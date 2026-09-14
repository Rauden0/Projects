package com.example.bubutracker.core.location

import android.Manifest
import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.Service
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Location
import android.os.Build
import android.os.IBinder
import android.util.Log
import androidx.core.app.ActivityCompat
import androidx.core.app.NotificationCompat
import androidx.core.content.ContextCompat
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.LocationUpdateData
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LocationService : Service() {

    private lateinit var fusedLocationClient: com.google.android.gms.location.FusedLocationProviderClient
    private lateinit var locationCallback: LocationCallback
    private var lastUploaded: Location? = null

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onCreate() {
        super.onCreate()
        createNotificationChannel()
        startForeground(NOTIFICATION_ID, buildNotification())
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)
        startLocationUpdates()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int = START_STICKY

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "Location tracking",
                NotificationManager.IMPORTANCE_LOW
            )
            val manager = getSystemService(NotificationManager::class.java)
            manager.createNotificationChannel(channel)
        }
    }

    private fun buildNotification(): Notification =
        NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle(getString(R.string.location_notification_title))
            .setContentText(getString(R.string.location_notification_text))
            .setSmallIcon(R.drawable.ic_notification_location)
            .setOngoing(true)
            .setPriority(NotificationCompat.PRIORITY_LOW)
            .build()

    private fun startLocationUpdates() {
        val locationRequest = LocationRequest.Builder(
            Priority.PRIORITY_BALANCED_POWER_ACCURACY,
            UPDATE_INTERVAL_MS
        )
            .setMinUpdateIntervalMillis(MIN_UPDATE_INTERVAL_MS)
            .setMinUpdateDistanceMeters(MIN_UPDATE_DISTANCE_M)
            .setMaxUpdateDelayMillis(MAX_UPDATE_DELAY_MS)
            .build()

        locationCallback = object : LocationCallback() {
            override fun onLocationResult(locationResult: LocationResult) {
                val location = locationResult.lastLocation ?: return
                if (shouldUpload(location)) {
                    lastUploaded = location
                    sendLocationToServer(location)
                }
            }
        }

        if (ActivityCompat.checkSelfPermission(
                this,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) == PackageManager.PERMISSION_GRANTED
        ) {
            fusedLocationClient.requestLocationUpdates(
                locationRequest,
                locationCallback,
                mainLooper
            )
        } else {
            stopSelf()
        }
    }

    private fun shouldUpload(location: Location): Boolean {
        val previous = lastUploaded ?: return true
        if (location.distanceTo(previous) >= MIN_UPDATE_DISTANCE_M) {
            return true
        }
        return location.time - previous.time >= FORCE_UPLOAD_INTERVAL_MS
    }

    private fun sendLocationToServer(location: Location) {
        ApiClient.service.updateLocation(
            LocationUpdateData(location.latitude, location.longitude)
        ).enqueue(object : Callback<Void> {
            override fun onResponse(call: Call<Void>, response: Response<Void>) {
                if (!response.isSuccessful) {
                    // Revert so the next location fix retries instead of silently
                    // treating this point as already uploaded.
                    lastUploaded = null
                    Log.w(TAG, "Location upload rejected: HTTP ${response.code()}")
                }
            }

            override fun onFailure(call: Call<Void>, t: Throwable) {
                lastUploaded = null
                Log.w(TAG, "Location upload failed", t)
            }
        })
    }

    override fun onDestroy() {
        super.onDestroy()
        if (::locationCallback.isInitialized) {
            fusedLocationClient.removeLocationUpdates(locationCallback)
        }
    }

    companion object {
        private const val TAG = "LocationService"
        private const val CHANNEL_ID = "bubutracker_location"
        private const val NOTIFICATION_ID = 1001
        private const val UPDATE_INTERVAL_MS = 30_000L
        private const val MIN_UPDATE_INTERVAL_MS = 15_000L
        private const val MIN_UPDATE_DISTANCE_M = 40f
        private const val MAX_UPDATE_DELAY_MS = 60_000L
        private const val FORCE_UPLOAD_INTERVAL_MS = 5 * 60_000L

        fun start(context: Context) {
            if (ActivityCompat.checkSelfPermission(
                    context,
                    Manifest.permission.ACCESS_FINE_LOCATION
                ) != PackageManager.PERMISSION_GRANTED
            ) {
                return
            }
            val intent = Intent(context, LocationService::class.java)
            try {
                ContextCompat.startForegroundService(context, intent)
            } catch (_: Exception) {
                // Android 12+ may block FGS from some background entry points; map/app open retries.
            }
        }

        fun stop(context: Context) {
            context.stopService(Intent(context, LocationService::class.java))
        }
    }
}
