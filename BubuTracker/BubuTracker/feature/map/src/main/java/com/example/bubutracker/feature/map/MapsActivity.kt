package com.example.bubutracker.feature.map

import android.Manifest
import android.annotation.SuppressLint
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.example.bubutracker.core.location.LocationService
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.AddTrackingDto
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TrackedLocationDto
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.session.SessionHolder
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.OnMapReadyCallback
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.BitmapDescriptorFactory
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.Marker
import com.google.android.gms.maps.model.MarkerOptions
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class MapsActivity : AppCompatActivity(), OnMapReadyCallback {

    private lateinit var mMap: GoogleMap
    private val locationPermissionRequestCode = 1
    private var isCameraMovedToUserLocation = false
    private val trackedMarkers = mutableMapOf<String, Marker>()
    private val pollHandler = Handler(Looper.getMainLooper())
    private val pollRunnable = object : Runnable {
        override fun run() {
            refreshTrackedLocations()
            pollHandler.postDelayed(this, POLL_INTERVAL_MS)
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        if (!SessionHolder.instance.isLoggedIn()) {
            navigateTo(AppActions.ACTION_LOGIN, finishCurrent = true)
            return
        }

        setContentView(R.layout.activity_map)

        findViewById<Button>(R.id.addTrackingButton).setOnClickListener { showAddTrackingDialog() }
        findViewById<Button>(R.id.logoutButton).setOnClickListener { logout() }
        findViewById<Button>(R.id.trackingRequestsButton).setOnClickListener { showIncomingRequestsDialog() }
        findViewById<Button>(R.id.followersButton).setOnClickListener { showFollowersDialog() }
        findViewById<Button>(R.id.peopleITrackButton).setOnClickListener { showTrackedUsersDialog() }

        val mapFragment = supportFragmentManager.findFragmentById(R.id.mapFragment) as? SupportMapFragment
        mapFragment?.getMapAsync(this) ?: throw NullPointerException("MapFragment is null")
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

    override fun onMapReady(googleMap: GoogleMap) {
        mMap = googleMap
        applyMapChromePadding()
        if (hasLocationPermission()) {
            enableMyLocation()
        } else {
            requestLocationPermissions()
        }
    }

    // Without this, Maps' own controls (compass, My Location button, logo, zoom
    // controls) render underneath our overlay buttons instead of avoiding them -
    // padding tells Maps to keep its controls clear of these edges.
    private fun applyMapChromePadding() {
        val density = resources.displayMetrics.density
        val topPx = (TOP_BUTTONS_HEIGHT_DP * density).toInt()
        val bottomPx = (BOTTOM_BUTTONS_HEIGHT_DP * density).toInt()
        mMap.setPadding(0, topPx, 0, bottomPx)
    }

    private fun hasLocationPermission(): Boolean =
        ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) ==
            PackageManager.PERMISSION_GRANTED

    private fun requestLocationPermissions() {
        val permissions = mutableListOf(
            Manifest.permission.ACCESS_FINE_LOCATION,
            Manifest.permission.ACCESS_COARSE_LOCATION
        )
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            permissions.add(Manifest.permission.ACCESS_BACKGROUND_LOCATION)
        }
        ActivityCompat.requestPermissions(this, permissions.toTypedArray(), locationPermissionRequestCode)
    }

    // Lint's permission checker doesn't trace the check through hasLocationPermission();
    // it genuinely is verified below before the location APIs are touched.
    @SuppressLint("MissingPermission")
    private fun enableMyLocation() {
        if (!hasLocationPermission()) {
            return
        }
        mMap.isMyLocationEnabled = true
        mMap.setOnMyLocationChangeListener { location ->
            if (!isCameraMovedToUserLocation) {
                val currentLatLng = LatLng(location.latitude, location.longitude)
                mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(currentLatLng, 15f))
                isCameraMovedToUserLocation = true
            }
        }
    }

    private fun startLocationServiceIfPermitted() {
        if (!hasLocationPermission()) {
            return
        }
        val serviceIntent = Intent(this, LocationService::class.java)
        ContextCompat.startForegroundService(this, serviceIntent)
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
                    if (!::mMap.isInitialized) {
                        return
                    }
                    updateMarkers(locations)
                }

                override fun onFailure(call: Call<List<TrackedLocationDto>>, t: Throwable) = Unit
            })
    }

    private fun updateMarkers(locations: List<TrackedLocationDto>) {
        val activeIds = locations.map { it.userId }.toSet()
        val iterator = trackedMarkers.entries.iterator()
        while (iterator.hasNext()) {
            val entry = iterator.next()
            if (entry.key !in activeIds) {
                entry.value.remove()
                iterator.remove()
            }
        }

        for (location in locations) {
            val position = LatLng(location.latitude, location.longitude)
            val title = listOf(location.firstName, location.lastName)
                .filter { it.isNotBlank() }
                .joinToString(" ")
                .ifBlank { location.email }

            val existingMarker = trackedMarkers[location.userId]
            if (existingMarker == null) {
                trackedMarkers[location.userId] = mMap.addMarker(
                    MarkerOptions()
                        .position(position)
                        .title(title)
                        .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_ORANGE))
                )!!
            } else {
                existingMarker.position = position
                existingMarker.title = title
            }
        }
    }

    private fun showAddTrackingDialog() {
        val input = EditText(this)
        input.hint = getString(R.string.add_tracking_email_hint)

        AlertDialog.Builder(this)
            .setTitle(R.string.add_tracking)
            .setView(input)
            .setPositiveButton(R.string.add) { _, _ ->
                val email = input.text.toString().trim()
                if (email.isNotEmpty()) {
                    addTracking(email)
                }
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun addTracking(email: String) {
        ApiClient.service.addTracking(AddTrackingDto(email))
            .enqueue(object : Callback<Void> {
                override fun onResponse(call: Call<Void>, response: Response<Void>) {
                    val message = if (response.isSuccessful) {
                        getString(R.string.tracking_added)
                    } else {
                        getString(R.string.tracking_add_failed, response.code())
                    }
                    Toast.makeText(this@MapsActivity, message, Toast.LENGTH_SHORT).show()
                    if (response.isSuccessful) {
                        refreshTrackedLocations()
                    }
                }

                override fun onFailure(call: Call<Void>, t: Throwable) {
                    Toast.makeText(
                        this@MapsActivity,
                        getString(R.string.network_error, t.message),
                        Toast.LENGTH_SHORT
                    ).show()
                }
            })
    }

    // Reject and revoke both reduce to "delete this (tracker, me) edge", which the
    // backend's DELETE /tracking/followers/{trackerId} handles regardless of whether
    // the edge is still pending or already accepted - so both actions call the same
    // removeFollower endpoint.
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
                showPersonListDialog(R.string.tracking_requests, requests) { requester ->
                    showAcceptRejectDialog(requester)
                }
            }

            override fun onFailure(call: Call<List<UserProfileDto>>, t: Throwable) = showNetworkErrorToast(t)
        })
    }

    private fun showAcceptRejectDialog(requester: UserProfileDto) {
        AlertDialog.Builder(this)
            .setTitle(R.string.accept_request_title)
            .setMessage(getString(R.string.accept_request_message, displayName(requester)))
            .setPositiveButton(R.string.accept) { _, _ ->
                ApiClient.service.acceptTrackingRequest(requester.id)
                    .enqueue(simpleResultCallback(R.string.request_accepted))
            }
            .setNegativeButton(R.string.reject) { _, _ ->
                ApiClient.service.removeFollower(requester.id)
                    .enqueue(simpleResultCallback(R.string.request_rejected))
            }
            .setNeutralButton(android.R.string.cancel, null)
            .show()
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
        AlertDialog.Builder(this)
            .setTitle(R.string.revoke_follower_title)
            .setMessage(getString(R.string.revoke_follower_message, displayName(follower)))
            .setPositiveButton(R.string.revoke) { _, _ ->
                ApiClient.service.removeFollower(follower.id)
                    .enqueue(simpleResultCallback(R.string.follower_revoked))
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun showTrackedUsersDialog() {
        ApiClient.service.getTrackedUsers().enqueue(object : Callback<List<UserProfileDto>> {
            override fun onResponse(
                call: Call<List<UserProfileDto>>,
                response: Response<List<UserProfileDto>>
            ) {
                if (!response.isSuccessful) {
                    showActionFailedToast(response.code())
                    return
                }
                val tracked = response.body().orEmpty()
                if (tracked.isEmpty()) {
                    Toast.makeText(this@MapsActivity, R.string.no_tracked_users, Toast.LENGTH_SHORT).show()
                    return
                }
                showPersonListDialog(R.string.people_i_track, tracked) { user ->
                    showStopTrackingDialog(user)
                }
            }

            override fun onFailure(call: Call<List<UserProfileDto>>, t: Throwable) = showNetworkErrorToast(t)
        })
    }

    private fun showStopTrackingDialog(user: UserProfileDto) {
        AlertDialog.Builder(this)
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
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun showPersonListDialog(titleRes: Int, people: List<UserProfileDto>, onPick: (UserProfileDto) -> Unit) {
        val labels = people.map { displayName(it) }.toTypedArray()
        AlertDialog.Builder(this)
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
        Toast.makeText(this, getString(R.string.action_failed, code), Toast.LENGTH_SHORT).show()
    }

    private fun showNetworkErrorToast(t: Throwable) {
        Toast.makeText(this, getString(R.string.network_error, t.message), Toast.LENGTH_SHORT).show()
    }

    private fun logout() {
        stopService(Intent(this, LocationService::class.java))
        SessionHolder.instance.clear()
        navigateTo(AppActions.ACTION_LOGIN, finishCurrent = true)
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        if (requestCode == locationPermissionRequestCode &&
            grantResults.isNotEmpty() &&
            grantResults[0] == PackageManager.PERMISSION_GRANTED
        ) {
            if (::mMap.isInitialized) {
                enableMyLocation()
            }
            startLocationServiceIfPermitted()
            return
        }
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
    }

    companion object {
        private const val POLL_INTERVAL_MS = 15_000L

        // Matches the top button stack in activity_map.xml: 24dp top margin + three
        // 40dp buttons (Requests/Followers/People I track) + two 8dp gaps, plus some
        // breathing room so Maps' compass/My Location button clear the last button.
        private const val TOP_BUTTONS_HEIGHT_DP = 176

        // Matches the bottom button row: 24dp bottom margin + 52dp button height,
        // plus breathing room so the Google logo clears it.
        private const val BOTTOM_BUTTONS_HEIGHT_DP = 92
    }
}
