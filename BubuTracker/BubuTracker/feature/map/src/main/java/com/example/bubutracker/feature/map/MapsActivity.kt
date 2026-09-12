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
        if (hasLocationPermission()) {
            enableMyLocation()
        } else {
            requestLocationPermissions()
        }
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
    }
}
