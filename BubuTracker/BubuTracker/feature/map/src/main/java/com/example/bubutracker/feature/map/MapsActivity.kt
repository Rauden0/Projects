package com.example.bubutracker.feature.map

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Paint
import android.graphics.Typeface
import android.graphics.drawable.BitmapDrawable
import android.os.Build
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.text.format.DateUtils
import android.util.LruCache
import android.view.View
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.PopupMenu
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.example.bubutracker.core.location.LocationService
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.AddTrackingDto
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TrackedLocationDto
import com.example.bubutracker.core.network.TrackedLocationsStore
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.network.UserProfileStore
import com.example.bubutracker.core.session.SessionHolder
import com.google.android.material.bottomsheet.BottomSheetDialog
import com.google.android.material.button.MaterialButton
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.google.android.material.textfield.TextInputEditText
import com.google.android.material.textfield.TextInputLayout
import org.osmdroid.config.Configuration
import org.osmdroid.tileprovider.tilesource.TileSourceFactory
import org.osmdroid.util.GeoPoint
import org.osmdroid.views.MapView
import org.osmdroid.views.overlay.Marker
import org.osmdroid.views.overlay.mylocation.GpsMyLocationProvider
import org.osmdroid.views.overlay.mylocation.MyLocationNewOverlay
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.SimpleDateFormat
import java.util.Locale
import java.util.TimeZone

class MapsActivity : AppCompatActivity() {

    private lateinit var mapView: MapView
    private var myLocationOverlay: MyLocationNewOverlay? = null
    private val locationPermissionRequestCode = 1
    private val notificationPermissionRequestCode = 2
    private var askedNotificationPermission = false
    private var hasCenteredOnUserLocation = false
    private var myInitials = ""
    private var myMarkerColor = DEFAULT_MARKER_COLOR
    private val trackedMarkers = mutableMapOf<String, Marker>()

    // Content-addressed by (initials, colorHex, sizePx), so a name or marker
    // color change is a cache miss (new key) rather than something that needs
    // explicit invalidation. Sized in bytes, not entry count, since each entry
    // is a real ARGB_8888 Bitmap. Instance-scoped: survives in-app navigation
    // (e.g. Profile and back) while the Activity is alive, and is naturally
    // dropped on logout/destroy instead of needing an explicit clear.
    private val markerBitmapCache = object : LruCache<String, Bitmap>(MARKER_BITMAP_CACHE_BYTES) {
        override fun sizeOf(key: String, value: Bitmap): Int = value.byteCount
    }

    // Nominatim is rate-limit sensitive and place names rarely change - avoid
    // one HTTP round trip per bottom-sheet open for the same rounded spot.
    private val geocodeCache = LruCache<String, GeocodeCacheEntry>(GEOCODE_CACHE_ENTRIES)
    private lateinit var trackedPeopleList: LinearLayout
    private val pollHandler = Handler(Looper.getMainLooper())
    private val pollRunnable = object : Runnable {
        override fun run() {
            refreshTrackedLocations()
            pollHandler.postDelayed(this, POLL_INTERVAL_MS)
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        // Must set user agent before creating any MapView (osmdroid throttles empty UA).
        Configuration.getInstance().userAgentValue = packageName
        super.onCreate(savedInstanceState)

        if (!SessionHolder.instance.isLoggedIn()) {
            navigateTo(AppActions.ACTION_LOGIN, finishCurrent = true)
            return
        }

        setContentView(R.layout.activity_map)

        findViewById<ImageButton>(R.id.logoutButton).setOnClickListener { logout() }
        findViewById<ImageButton>(R.id.manageButton).setOnClickListener { showManageMenu(it) }
        findViewById<ImageButton>(R.id.profileButton).setOnClickListener { navigateTo(AppActions.ACTION_PROFILE) }
        trackedPeopleList = findViewById(R.id.trackedPeopleList)
        fetchOwnProfile()

        mapView = findViewById(R.id.mapView)
        mapView.setTileSource(TileSourceFactory.MAPNIK)
        mapView.setMultiTouchControls(true)
        mapView.setBuiltInZoomControls(false)
        mapView.controller.setZoom(15.0)
        mapView.controller.setCenter(GeoPoint(0.0, 0.0))

        // Paint the last known snapshot immediately so a cold start doesn't
        // show a blank map for the second or two the first live poll takes;
        // refreshTrackedLocations() (from onStart) overwrites this shortly.
        TrackedLocationsStore.getCached()?.let { cached ->
            updateMarkers(cached)
            updateTrackedPeopleList(cached)
        }

        if (hasLocationPermission()) {
            enableMyLocation()
        } else {
            requestLocationPermissions()
        }
    }

    override fun onResume() {
        super.onResume()
        mapView.onResume()
        if (hasLocationPermission()) {
            myLocationOverlay?.enableMyLocation()
        }
    }

    override fun onPause() {
        myLocationOverlay?.disableMyLocation()
        mapView.onPause()
        super.onPause()
    }

    override fun onStart() {
        super.onStart()
        pollHandler.post(pollRunnable)
        startLocationServiceIfPermitted()
    }

    override fun onStop() {
        super.onStop()
        pollHandler.removeCallbacks(pollRunnable)
    }

    private fun hasLocationPermission(): Boolean =
        ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) ==
            PackageManager.PERMISSION_GRANTED

    private fun requestLocationPermissions() {
        ActivityCompat.requestPermissions(
            this,
            arrayOf(
                Manifest.permission.ACCESS_FINE_LOCATION,
                Manifest.permission.ACCESS_COARSE_LOCATION
            ),
            locationPermissionRequestCode
        )
    }

    private fun enableMyLocation() {
        if (!hasLocationPermission()) {
            return
        }
        val provider = GpsMyLocationProvider(this).apply {
            locationUpdateMinTime = MAP_LOCATION_MIN_TIME_MS
            locationUpdateMinDistance = MAP_LOCATION_MIN_DISTANCE_M
        }
        val overlay = myLocationOverlay ?: MyLocationNewOverlay(provider, mapView).apply {
            setPersonIcon(createMarkerBitmap(myInitials, myMarkerColor))
            setPersonAnchor(0.5f, 0.5f)
            enableMyLocation()
            runOnFirstFix {
                if (!hasCenteredOnUserLocation) {
                    hasCenteredOnUserLocation = true
                    runOnUiThread {
                        myLocation?.let { mapView.controller.animateTo(it) }
                    }
                }
            }
            mapView.overlays.add(this)
        }
        myLocationOverlay = overlay
    }

    private fun startLocationServiceIfPermitted() {
        if (!hasLocationPermission()) {
            return
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU &&
            ContextCompat.checkSelfPermission(this, Manifest.permission.POST_NOTIFICATIONS) !=
            PackageManager.PERMISSION_GRANTED &&
            !askedNotificationPermission
        ) {
            askedNotificationPermission = true
            ActivityCompat.requestPermissions(
                this,
                arrayOf(Manifest.permission.POST_NOTIFICATIONS),
                notificationPermissionRequestCode
            )
            return
        }
        startForegroundLocationService()
    }

    private fun startForegroundLocationService() {
        LocationService.start(this)
    }

    private fun refreshTrackedLocations() {
        ApiClient.service.getTrackedLocations()
            .enqueue(object : Callback<List<TrackedLocationDto>> {
                override fun onResponse(
                    call: Call<List<TrackedLocationDto>>,
                    response: Response<List<TrackedLocationDto>>
                ) {
                    if (!response.isSuccessful) {
                        return
                    }
                    val locations = response.body() ?: return
                    updateMarkers(locations)
                    updateTrackedPeopleList(locations)
                    TrackedLocationsStore.set(locations)
                }

                override fun onFailure(call: Call<List<TrackedLocationDto>>, t: Throwable) = Unit
            })
    }

    private fun updateMarkers(locations: List<TrackedLocationDto>) {
        val withCoords = locations.filter { it.latitude != null && it.longitude != null }
        val activeIds = withCoords.map { it.userId }.toSet()
        val iterator = trackedMarkers.entries.iterator()
        while (iterator.hasNext()) {
            val entry = iterator.next()
            if (entry.key !in activeIds) {
                mapView.overlays.remove(entry.value)
                iterator.remove()
            }
        }

        for (location in withCoords) {
            val position = GeoPoint(location.latitude!!, location.longitude!!)
            val title = listOf(location.firstName, location.lastName)
                .filter { it.isNotBlank() }
                .joinToString(" ")
                .ifBlank { location.email }

            val existingMarker = trackedMarkers[location.userId]
            if (existingMarker == null) {
                val marker = Marker(mapView).apply {
                    this.position = position
                    this.title = title
                    setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_CENTER)
                    icon = BitmapDrawable(resources, createMarkerBitmap(initialsFor(location), location.markerColor))
                    relatedObject = location
                    setOnMarkerClickListener { clickedMarker, _ ->
                        showLastSeenBottomSheet(clickedMarker.relatedObject as TrackedLocationDto)
                        true
                    }
                }
                trackedMarkers[location.userId] = marker
                mapView.overlays.add(marker)
            } else {
                existingMarker.position = position
                existingMarker.title = title
                existingMarker.relatedObject = location
                existingMarker.icon = BitmapDrawable(resources, createMarkerBitmap(initialsFor(location), location.markerColor))
            }
        }
        mapView.invalidate()
    }

    private fun createMarkerBitmap(initials: String, colorHex: String): Bitmap {
        val sizePx = (MARKER_SIZE_DP * resources.displayMetrics.density).toInt()
        val key = "$initials|$colorHex|$sizePx"
        markerBitmapCache.get(key)?.let { return it }
        return drawMarkerBitmap(initials, colorHex, sizePx).also { markerBitmapCache.put(key, it) }
    }

    private fun drawMarkerBitmap(initials: String, colorHex: String, sizePx: Int): Bitmap {
        val bitmap = Bitmap.createBitmap(sizePx, sizePx, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bitmap)
        val markerColor = parseMarkerColor(colorHex)

        // Tint recolors the eye too — redraw the white eye afterwards.
        ContextCompat.getDrawable(this, R.drawable.ic_tracked_marker)?.apply {
            setBounds(0, 0, sizePx, sizePx)
            colorFilter = android.graphics.PorterDuffColorFilter(markerColor, android.graphics.PorterDuff.Mode.SRC_IN)
            draw(canvas)
        }

        // Eye position/radius from ic_tracked_marker.xml's 24x24 viewport.
        val eyePaint = Paint(Paint.ANTI_ALIAS_FLAG).apply { color = android.graphics.Color.WHITE }
        canvas.drawCircle(sizePx * (18.1f / 24f), sizePx * (8.1f / 24f), sizePx * (0.85f / 24f), eyePaint)

        // Torso center in the drawable's 24x24 viewport.
        val cx = sizePx * (10.2f / 24f)
        val cy = sizePx * (14f / 24f)

        val strokePaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = android.graphics.Color.BLACK
            style = Paint.Style.STROKE
            strokeWidth = sizePx * 0.05f
            strokeJoin = Paint.Join.ROUND
            textSize = sizePx * 0.3f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.CENTER
        }
        val fillPaint = Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = android.graphics.Color.WHITE
            style = Paint.Style.FILL
            textSize = sizePx * 0.3f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.CENTER
        }
        val textY = cy - (fillPaint.descent() + fillPaint.ascent()) / 2f
        canvas.drawText(initials, cx, textY, strokePaint)
        canvas.drawText(initials, cx, textY, fillPaint)

        return bitmap
    }

    private fun parseMarkerColor(colorHex: String): Int =
        try {
            android.graphics.Color.parseColor(colorHex)
        } catch (e: IllegalArgumentException) {
            android.graphics.Color.parseColor(DEFAULT_MARKER_COLOR)
        }

    private fun initialsFor(location: TrackedLocationDto): String =
        initials(location.firstName, location.lastName, location.email)

    private fun initials(firstName: String, lastName: String, email: String): String {
        val computed = listOfNotNull(
            firstName.trim().firstOrNull(),
            lastName.trim().firstOrNull()
        ).joinToString("").uppercase()
        return computed.ifBlank { email.trim().take(1).uppercase() }
    }

    private fun showLastSeenBottomSheet(location: TrackedLocationDto) {
        val title = listOf(location.firstName, location.lastName)
            .filter { it.isNotBlank() }
            .joinToString(" ")
            .ifBlank { location.email }

        val sheet = BottomSheetDialog(this)
        val view = layoutInflater.inflate(R.layout.bottom_sheet_tracked_person, null)
        sheet.setContentView(view)

        val locationValueView = view.findViewById<TextView>(R.id.sheetLocationValue)
        view.findViewById<TextView>(R.id.sheetPersonName).text = title
        view.findViewById<TextView>(R.id.sheetLastSeenValue).text = formatLastSeen(location.updatedAt)
        view.findViewById<MaterialButton>(R.id.sheetStopTrackingButton).setOnClickListener {
            sheet.dismiss()
            showStopTrackingDialog(
                UserProfileDto(
                    id = location.userId,
                    email = location.email,
                    firstName = location.firstName,
                    lastName = location.lastName,
                    markerColor = location.markerColor
                )
            )
        }

        val lat = location.latitude
        val lon = location.longitude
        if (location.updatedAt == null || lat == null || lon == null) {
            locationValueView.text = getString(R.string.location_unknown)
        } else {
            locationValueView.text = getString(R.string.location_resolving)
            reverseGeocode(lat, lon) { placeName ->
                if (sheet.isShowing) {
                    locationValueView.text = placeName ?: getString(R.string.location_unknown)
                }
            }
        }

        sheet.show()
    }

    private fun reverseGeocode(latitude: Double, longitude: Double, onResult: (String?) -> Unit) {
        val key = geocodeCacheKey(latitude, longitude)
        geocodeCache.get(key)?.let { cached ->
            if (System.currentTimeMillis() - cached.cachedAtMillis < GEOCODE_CACHE_TTL_MS) {
                onResult(cached.placeName)
                return
            }
        }

        Thread {
            val placeName = try {
                val url = java.net.URL(
                    "https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=$latitude&lon=$longitude&zoom=16&addressdetails=0"
                )
                val connection = (url.openConnection() as java.net.HttpURLConnection).apply {
                    setRequestProperty("User-Agent", packageName)
                    connectTimeout = REVERSE_GEOCODE_TIMEOUT_MS
                    readTimeout = REVERSE_GEOCODE_TIMEOUT_MS
                }
                val body = connection.inputStream.bufferedReader().use { it.readText() }
                connection.disconnect()
                org.json.JSONObject(body).optString("display_name").ifBlank { null }?.let(::shortenPlaceName)
            } catch (e: Exception) {
                null
            }
            // Only successful lookups are cached - caching a failure would hide
            // a transient network error behind "Unknown location" for the TTL.
            if (placeName != null) {
                geocodeCache.put(key, GeocodeCacheEntry(placeName, System.currentTimeMillis()))
            }
            runOnUiThread { onResult(placeName) }
        }.start()
    }

    // Rounds to ~11 m grid cells (4 decimal places) so nearby fixes for the
    // same tracked person share a cache entry instead of one per GPS jitter.
    private fun geocodeCacheKey(latitude: Double, longitude: Double): String {
        val roundedLat = Math.round(latitude * GEOCODE_ROUNDING_FACTOR) / GEOCODE_ROUNDING_FACTOR
        val roundedLon = Math.round(longitude * GEOCODE_ROUNDING_FACTOR) / GEOCODE_ROUNDING_FACTOR
        return "$roundedLat,$roundedLon"
    }

    private fun shortenPlaceName(displayName: String): String =
        displayName.split(",").map { it.trim() }.filter { it.isNotEmpty() }.take(2).joinToString(", ")

    private fun formatLastSeen(updatedAt: String?): String {
        if (updatedAt == null) {
            return getString(R.string.no_location_reported)
        }
        val millis = parseIsoInstantMillis(updatedAt) ?: return getString(R.string.no_location_reported)
        val now = System.currentTimeMillis()
        // Clamp to now so clock skew never shows "in X minutes".
        return DateUtils.getRelativeTimeSpanString(
            minOf(millis, now),
            now,
            DateUtils.MINUTE_IN_MILLIS
        ).toString()
    }

    // Go RFC3339Nano → millis: SimpleDateFormat needs exactly 3 fraction digits.
    private fun parseIsoInstantMillis(isoInstant: String): Long? {
        val normalized = isoInstant.replace(
            Regex("""(\.\d+)?Z$""")
        ) { match ->
            val fraction = match.groupValues[1].removePrefix(".").padEnd(3, '0').take(3)
            ".${fraction}Z"
        }.let { if (it.contains('.')) it else it.replace("Z", ".000Z") }

        val format = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.US).apply {
            timeZone = TimeZone.getTimeZone("UTC")
        }
        return try {
            format.parse(normalized)?.time
        } catch (e: java.text.ParseException) {
            null
        }
    }

    private fun showManageMenu(anchor: View) {
        val popup = PopupMenu(this, anchor)
        popup.menu.add(MENU_GROUP, MENU_ITEM_TRACK_SOMEONE, 0, R.string.add_tracking)
        popup.menu.add(MENU_GROUP, MENU_ITEM_REQUESTS, 1, R.string.tracking_requests)
        popup.menu.add(MENU_GROUP, MENU_ITEM_FOLLOWERS, 2, R.string.followers)
        popup.setOnMenuItemClickListener { item ->
            when (item.itemId) {
                MENU_ITEM_TRACK_SOMEONE -> showTrackSomeoneDialog()
                MENU_ITEM_REQUESTS -> showIncomingRequestsDialog()
                MENU_ITEM_FOLLOWERS -> showFollowersDialog()
            }
            true
        }
        popup.show()
    }

    private fun fetchOwnProfile() {
        UserProfileStore.getFresh()?.let { applyOwnProfile(it); return }

        ApiClient.service.getMe().enqueue(object : Callback<UserProfileDto> {
            override fun onResponse(call: Call<UserProfileDto>, response: Response<UserProfileDto>) {
                val profile = response.body() ?: return
                UserProfileStore.set(profile)
                applyOwnProfile(profile)
            }

            override fun onFailure(call: Call<UserProfileDto>, t: Throwable) = Unit
        })
    }

    private fun applyOwnProfile(profile: UserProfileDto) {
        if (profile.firstName.isBlank() && profile.lastName.isBlank()) {
            navigateTo(
                AppActions.ACTION_PROFILE,
                finishCurrent = true,
                extras = Bundle().apply { putBoolean(AppActions.EXTRA_REQUIRE_NAME, true) }
            )
            return
        }
        myInitials = initials(profile.firstName, profile.lastName, profile.email)
        myMarkerColor = profile.markerColor
        myLocationOverlay?.setPersonIcon(createMarkerBitmap(myInitials, myMarkerColor))
    }

    private fun showTrackSomeoneDialog() {
        val view = layoutInflater.inflate(R.layout.dialog_track_someone, null)
        val inputLayout = view.findViewById<TextInputLayout>(R.id.trackEmailLayout)
        val input = view.findViewById<TextInputEditText>(R.id.trackEmailInput)
        val pendingEmpty = view.findViewById<TextView>(R.id.pendingEmpty)
        val pendingList = view.findViewById<android.widget.ListView>(R.id.pendingList)

        inputLayout.boxBackgroundColor = android.graphics.Color.TRANSPARENT
        inputLayout.boxStrokeColor = ContextCompat.getColor(
            this,
            com.example.bubutracker.core.designsystem.R.color.brand_primary
        )

        val pendingPeople = mutableListOf<UserProfileDto>()
        val pendingLabels = mutableListOf<String>()
        val adapter = android.widget.ArrayAdapter(
            this,
            android.R.layout.simple_list_item_1,
            pendingLabels
        )
        pendingList.adapter = adapter

        fun bindPending(people: List<UserProfileDto>) {
            pendingPeople.clear()
            pendingPeople.addAll(people)
            pendingLabels.clear()
            pendingLabels.addAll(people.map { displayName(it) })
            adapter.notifyDataSetChanged()
            pendingEmpty.visibility = if (people.isEmpty()) android.view.View.VISIBLE else android.view.View.GONE
            pendingList.visibility = if (people.isEmpty()) android.view.View.GONE else android.view.View.VISIBLE
        }

        fun reloadPending() {
            ApiClient.service.getOutgoingTrackingRequests()
                .enqueue(object : Callback<List<UserProfileDto>> {
                    override fun onResponse(
                        call: Call<List<UserProfileDto>>,
                        response: Response<List<UserProfileDto>>
                    ) {
                        bindPending(if (response.isSuccessful) response.body().orEmpty() else emptyList())
                    }

                    override fun onFailure(call: Call<List<UserProfileDto>>, t: Throwable) {
                        bindPending(emptyList())
                    }
                })
        }

        reloadPending()

        pendingList.setOnItemClickListener { _, _, position, _ ->
            showPendingPersonSheet(pendingPeople[position])
        }

        val dialog = MaterialAlertDialogBuilder(this)
            .setTitle(R.string.add_tracking)
            .setView(view)
            .setPositiveButton(R.string.add, null)
            .setNegativeButton(R.string.cancel, null)
            .create()

        dialog.setOnShowListener {
            dialog.getButton(androidx.appcompat.app.AlertDialog.BUTTON_POSITIVE).setOnClickListener {
                val email = input.text?.toString()?.trim().orEmpty()
                if (email.isEmpty()) {
                    return@setOnClickListener
                }
                addTracking(email, keepDialogOpen = true) {
                    input.text?.clear()
                    reloadPending()
                    refreshTrackedLocations()
                }
            }
        }
        dialog.show()
    }

    private fun addTracking(
        email: String,
        keepDialogOpen: Boolean = false,
        onSuccess: (() -> Unit)? = null
    ) {
        ApiClient.service.addTracking(AddTrackingDto(email))
            .enqueue(object : Callback<Void> {
                override fun onResponse(call: Call<Void>, response: Response<Void>) {
                    val message = when {
                        response.isSuccessful -> null
                        response.code() == 404 -> getString(R.string.tracking_add_not_found)
                        response.code() == 409 -> getString(R.string.tracking_add_already_exists)
                        response.code() == 400 -> getString(R.string.tracking_add_invalid)
                        response.code() == 429 -> getString(R.string.tracking_add_rate_limited)
                        response.code() == 401 -> getString(R.string.action_failed_unauthorized)
                        else -> getString(R.string.tracking_add_failed)
                    }
                    if (response.isSuccessful) {
                        if (!keepDialogOpen) {
                            Toast.makeText(this@MapsActivity, R.string.tracking_added, Toast.LENGTH_SHORT).show()
                            refreshTrackedLocations()
                        }
                        onSuccess?.invoke()
                    } else if (message != null) {
                        showErrorDialog(message)
                    }
                }

                override fun onFailure(call: Call<Void>, t: Throwable) {
                    showErrorDialog(getString(R.string.network_error))
                }
            })
    }

    private fun showIncomingRequestsDialog() {
        ApiClient.service.getIncomingTrackingRequests().enqueue(object : Callback<List<UserProfileDto>> {
            override fun onResponse(
                call: Call<List<UserProfileDto>>,
                response: Response<List<UserProfileDto>>
            ) {
                if (!response.isSuccessful) {
                    showActionFailedToast(response.code())
                    return
                }
                val requests = response.body().orEmpty()
                if (requests.isEmpty()) {
                    Toast.makeText(this@MapsActivity, R.string.no_pending_requests, Toast.LENGTH_SHORT).show()
                    return
                }

                val container = LinearLayout(this@MapsActivity).apply {
                    orientation = LinearLayout.VERTICAL
                    setPadding(0, (8 * resources.displayMetrics.density).toInt(), 0, 0)
                }
                val dialog = MaterialAlertDialogBuilder(this@MapsActivity)
                    .setTitle(R.string.tracking_requests)
                    .setView(container)
                    .setNegativeButton(R.string.cancel, null)
                    .create()

                fun refreshRows(people: List<UserProfileDto>) {
                    container.removeAllViews()
                    if (people.isEmpty()) {
                        dialog.dismiss()
                        Toast.makeText(this@MapsActivity, R.string.no_pending_requests, Toast.LENGTH_SHORT).show()
                        return
                    }
                    for (requester in people) {
                        val row = layoutInflater.inflate(R.layout.item_incoming_request, container, false)
                        row.findViewById<TextView>(R.id.personName).text = displayName(requester)
                        row.findViewById<TextView>(R.id.acceptButton).setOnClickListener {
                            ApiClient.service.acceptTrackingRequest(requester.id)
                                .enqueue(object : Callback<Void> {
                                    override fun onResponse(call: Call<Void>, response: Response<Void>) {
                                        if (response.isSuccessful) {
                                            Toast.makeText(
                                                this@MapsActivity,
                                                R.string.request_accepted,
                                                Toast.LENGTH_SHORT
                                            ).show()
                                            refreshRows(people.filter { it.id != requester.id })
                                        } else {
                                            showActionFailedToast(response.code())
                                        }
                                    }

                                    override fun onFailure(call: Call<Void>, t: Throwable) =
                                        showNetworkErrorToast(t)
                                })
                        }
                        row.findViewById<TextView>(R.id.rejectButton).setOnClickListener {
                            ApiClient.service.removeFollower(requester.id)
                                .enqueue(object : Callback<Void> {
                                    override fun onResponse(call: Call<Void>, response: Response<Void>) {
                                        if (response.isSuccessful) {
                                            Toast.makeText(
                                                this@MapsActivity,
                                                R.string.request_rejected,
                                                Toast.LENGTH_SHORT
                                            ).show()
                                            refreshRows(people.filter { it.id != requester.id })
                                        } else {
                                            showActionFailedToast(response.code())
                                        }
                                    }

                                    override fun onFailure(call: Call<Void>, t: Throwable) =
                                        showNetworkErrorToast(t)
                                })
                        }
                        container.addView(row)
                    }
                }

                refreshRows(requests)
                dialog.show()
            }

            override fun onFailure(call: Call<List<UserProfileDto>>, t: Throwable) = showNetworkErrorToast(t)
        })
    }

    private fun showFollowersDialog() {
        ApiClient.service.getFollowers().enqueue(object : Callback<List<UserProfileDto>> {
            override fun onResponse(
                call: Call<List<UserProfileDto>>,
                response: Response<List<UserProfileDto>>
            ) {
                if (!response.isSuccessful) {
                    showActionFailedToast(response.code())
                    return
                }
                val followers = response.body().orEmpty()
                if (followers.isEmpty()) {
                    Toast.makeText(this@MapsActivity, R.string.no_followers, Toast.LENGTH_SHORT).show()
                    return
                }
                showPersonListDialog(R.string.followers, followers) { follower ->
                    showRevokeDialog(follower)
                }
            }

            override fun onFailure(call: Call<List<UserProfileDto>>, t: Throwable) = showNetworkErrorToast(t)
        })
    }

    private fun showRevokeDialog(follower: UserProfileDto) {
        MaterialAlertDialogBuilder(this)
            .setTitle(R.string.revoke_follower_title)
            .setMessage(getString(R.string.revoke_follower_message, displayName(follower)))
            .setPositiveButton(R.string.revoke) { _, _ ->
                ApiClient.service.removeFollower(follower.id)
                    .enqueue(simpleResultCallback(R.string.follower_revoked))
            }
            .setNegativeButton(R.string.cancel, null)
            .show()
    }

    private fun centerMapOnTrackedUser(userId: String) {
        val marker = trackedMarkers[userId]
        if (marker == null) {
            Toast.makeText(this, R.string.no_location_reported, Toast.LENGTH_SHORT).show()
            return
        }
        mapView.controller.setZoom(CENTER_ON_PERSON_ZOOM)
        mapView.controller.animateTo(marker.position)
    }

    private fun updateTrackedPeopleList(accepted: List<TrackedLocationDto>) {
        trackedPeopleList.removeAllViews()
        val density = resources.displayMetrics.density
        val avatarSizePx = (AVATAR_SIZE_DP * density).toInt()
        val itemSpacingPx = (PEOPLE_LIST_ITEM_SPACING_DP * density).toInt()

        for (location in accepted) {
            addPersonChip(
                density = density,
                avatarSizePx = avatarSizePx,
                itemSpacingPx = itemSpacingPx,
                initials = initialsFor(location),
                markerColor = location.markerColor,
                label = location.firstName.ifBlank { location.email },
                dimmed = false,
                onClick = {
                    if (location.latitude != null && location.longitude != null) {
                        centerMapOnTrackedUser(location.userId)
                    }
                    showLastSeenBottomSheet(location)
                }
            )
        }
    }

    private fun addPersonChip(
        density: Float,
        avatarSizePx: Int,
        itemSpacingPx: Int,
        initials: String,
        markerColor: String,
        label: String,
        dimmed: Boolean,
        onClick: () -> Unit
    ) {
        val item = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = android.view.Gravity.CENTER_HORIZONTAL
            layoutParams = LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WRAP_CONTENT,
                LinearLayout.LayoutParams.WRAP_CONTENT
            ).apply { marginEnd = itemSpacingPx }
            setOnClickListener { onClick() }
        }

        val avatar = TextView(this).apply {
            layoutParams = LinearLayout.LayoutParams(avatarSizePx, avatarSizePx)
            gravity = android.view.Gravity.CENTER
            text = initials
            textSize = 16f
            setTextColor(android.graphics.Color.WHITE)
            setTypeface(typeface, Typeface.BOLD)
            background = android.graphics.drawable.GradientDrawable().apply {
                shape = android.graphics.drawable.GradientDrawable.OVAL
                setColor(parseMarkerColor(markerColor))
                setStroke((2 * density).toInt(), android.graphics.Color.WHITE)
            }
            alpha = if (dimmed) 0.55f else 1f
        }

        val labelView = TextView(this).apply {
            layoutParams = LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WRAP_CONTENT,
                LinearLayout.LayoutParams.WRAP_CONTENT
            ).apply { topMargin = (4 * density).toInt() }
            text = label
            textSize = 12f
            maxWidth = (avatarSizePx * 1.6f).toInt()
            maxLines = 1
            ellipsize = android.text.TextUtils.TruncateAt.END
            setTextColor(ContextCompat.getColor(this@MapsActivity, com.example.bubutracker.core.designsystem.R.color.on_surface))
        }

        item.addView(avatar)
        item.addView(labelView)
        trackedPeopleList.addView(item)
    }

    private fun showPendingPersonSheet(person: UserProfileDto) {
        val sheet = BottomSheetDialog(this)
        val view = layoutInflater.inflate(R.layout.bottom_sheet_tracked_person, null)
        sheet.setContentView(view)

        view.findViewById<TextView>(R.id.sheetPersonName).text = displayName(person)
        view.findViewById<TextView>(R.id.sheetLocationValue).text = getString(R.string.tracking_pending)
        view.findViewById<TextView>(R.id.sheetLastSeenValue).text = getString(R.string.tracking_pending_waiting)
        view.findViewById<MaterialButton>(R.id.sheetStopTrackingButton).setOnClickListener {
            sheet.dismiss()
            showStopTrackingDialog(person)
        }
        sheet.show()
    }

    private fun showStopTrackingDialog(user: UserProfileDto) {
        MaterialAlertDialogBuilder(this)
            .setTitle(R.string.stop_tracking_title)
            .setMessage(getString(R.string.stop_tracking_message, displayName(user)))
            .setPositiveButton(R.string.stop_tracking) { _, _ ->
                ApiClient.service.removeTracking(user.id).enqueue(object : Callback<Void> {
                    override fun onResponse(call: Call<Void>, response: Response<Void>) {
                        if (response.isSuccessful) {
                            Toast.makeText(this@MapsActivity, R.string.tracking_stopped, Toast.LENGTH_SHORT).show()
                            refreshTrackedLocations()
                        } else {
                            showActionFailedToast(response.code())
                        }
                    }

                    override fun onFailure(call: Call<Void>, t: Throwable) = showNetworkErrorToast(t)
                })
            }
            .setNegativeButton(R.string.cancel, null)
            .show()
    }

    private fun showPersonListDialog(titleRes: Int, people: List<UserProfileDto>, onPick: (UserProfileDto) -> Unit) {
        val labels = people.map { displayName(it) }.toTypedArray()
        MaterialAlertDialogBuilder(this)
            .setTitle(titleRes)
            .setItems(labels) { _, index -> onPick(people[index]) }
            .show()
    }

    private fun displayName(user: UserProfileDto): String =
        listOf(user.firstName, user.lastName)
            .filter { it.isNotBlank() }
            .joinToString(" ")
            .ifBlank { user.email }

    private fun simpleResultCallback(successMessageRes: Int) = object : Callback<Void> {
        override fun onResponse(call: Call<Void>, response: Response<Void>) {
            if (response.isSuccessful) {
                Toast.makeText(this@MapsActivity, successMessageRes, Toast.LENGTH_SHORT).show()
            } else {
                showActionFailedToast(response.code())
            }
        }

        override fun onFailure(call: Call<Void>, t: Throwable) = showNetworkErrorToast(t)
    }

    private fun showActionFailedToast(code: Int) {
        val message = when (code) {
            404 -> getString(R.string.action_failed_not_found)
            401 -> getString(R.string.action_failed_unauthorized)
            429 -> getString(R.string.action_failed_rate_limited)
            else -> getString(R.string.action_failed)
        }
        showErrorDialog(message)
    }

    private fun showNetworkErrorToast(t: Throwable) {
        showErrorDialog(getString(R.string.network_error))
    }

    private fun showErrorDialog(message: String) {
        MaterialAlertDialogBuilder(this)
            .setMessage(message)
            .setPositiveButton(R.string.ok, null)
            .show()
    }

    private fun logout() {
        LocationService.stop(this)
        SessionHolder.instance.clear()
        UserProfileStore.clear()
        TrackedLocationsStore.clear()
        // Login must also clear Auth0 browser SSO.
        val intent = Intent(AppActions.ACTION_LOGIN)
            .setPackage(packageName)
            .putExtra(AppActions.EXTRA_PERFORM_AUTH0_LOGOUT, true)
        startActivity(intent)
        finish()
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        when (requestCode) {
            locationPermissionRequestCode -> {
                if (grantResults.isNotEmpty() &&
                    grantResults[0] == PackageManager.PERMISSION_GRANTED
                ) {
                    enableMyLocation()
                    startLocationServiceIfPermitted()
                }
            }
            notificationPermissionRequestCode -> {
                // Ask first; start FGS either way so sharing still works if the user denies.
                startForegroundLocationService()
            }
            else -> super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        }
    }

    companion object {
        private const val POLL_INTERVAL_MS = 30_000L
        private const val MAP_LOCATION_MIN_TIME_MS = 15_000L
        private const val MAP_LOCATION_MIN_DISTANCE_M = 25f
        private const val MARKER_SIZE_DP = 44
        // A handful of distinct (initials, color) combos at most; way more than needed.
        private const val MARKER_BITMAP_CACHE_BYTES = 1024 * 1024
        private const val DEFAULT_MARKER_COLOR = "#FF9800"
        private const val CENTER_ON_PERSON_ZOOM = 16.0
        private const val REVERSE_GEOCODE_TIMEOUT_MS = 5_000
        private const val GEOCODE_CACHE_ENTRIES = 200
        private const val GEOCODE_CACHE_TTL_MS = 6 * 60 * 60 * 1000L
        private const val GEOCODE_ROUNDING_FACTOR = 10_000.0
        private const val AVATAR_SIZE_DP = 48
        private const val PEOPLE_LIST_ITEM_SPACING_DP = 16
        private const val MENU_GROUP = 0
        private const val MENU_ITEM_TRACK_SOMEONE = 1
        private const val MENU_ITEM_REQUESTS = 2
        private const val MENU_ITEM_FOLLOWERS = 3
    }
}

private data class GeocodeCacheEntry(val placeName: String, val cachedAtMillis: Long)
