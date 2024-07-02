import android.Manifest
import android.app.Service
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Location
import android.os.IBinder
import androidx.core.app.ActivityCompat
import com.google.android.gms.location.*
import java.util.Date

@Suppress("DEPRECATION")
class LocationService : Service() {

    private lateinit var fusedLocationClient: FusedLocationProviderClient
    private lateinit var locationRequest: LocationRequest
    private lateinit var locationCallback: LocationCallback

    override fun onBind(intent: Intent?): IBinder? {
        return null
    }

    override fun onCreate() {
        super.onCreate()
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)
        createLocationRequest()
        startLocationUpdates()
    }

    private fun createLocationRequest() {
        locationRequest = LocationRequest.create().apply {
            interval = 10000 // 10 seconds
            fastestInterval = 5000 // 5 seconds
            priority = LocationRequest.PRIORITY_HIGH_ACCURACY
        }
    }

    private fun startLocationUpdates() {
        locationCallback = object : LocationCallback() {
            fun onLocationResult(locationResult: LocationResult?) {
                locationResult ?: return
                for (location in locationResult.locations) {
                    // Handle location updates here
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
                null
            )
        }
    }

    private fun sendLocationToServer(location: Location) {
        // val retrofit = Retrofit.Builder()
        //     .baseUrl(BASE_URL)
        //     .addConverterFactory(GsonConverterFactory.create())
        //     .build()
        // val service = retrofit.create(ApiService::class.java)
        // val call = service.sendLocationUpdate(LocationUpdateDto(userId, location.latitude, location.longitude, Date()))
        // call.enqueue(object : Callback<Void> {
        //     override fun onResponse(call: Call<Void>, response: Response<Void>) {
        //         // Handle successful response
        //     }
        //
        //     override fun onFailure(call: Call<Void>, t: Throwable) {
        //         // Handle failure
        //     }
        // })
    }

    override fun onDestroy() {
        super.onDestroy()
        fusedLocationClient.removeLocationUpdates(locationCallback)
    }

    companion object {
        private const val BASE_URL = "https://your-server-url.com/api/"

        // Example of LocationUpdateDto
        data class LocationUpdateDto(
            val userId: String,
            val latitude: Double,
            val longitude: Double,
            val timestamp: Date
        )
    }
}
