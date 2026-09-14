package com.example.bubutracker

import android.app.Application
import android.content.Intent
import androidx.lifecycle.DefaultLifecycleObserver
import androidx.lifecycle.LifecycleOwner
import androidx.lifecycle.ProcessLifecycleOwner
import com.example.bubutracker.core.location.LocationService
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.SessionAuthenticator
import com.example.bubutracker.core.network.TrackedLocationsStore
import com.example.bubutracker.core.network.UserProfileStore
import com.example.bubutracker.core.session.SessionHolder
import com.example.bubutracker.feature.auth.Auth0TokenRefresher

class BubuTrackerApp : Application() {
    override fun onCreate() {
        super.onCreate()
        SessionHolder.init(this)
        TrackedLocationsStore.init(this)

        val authenticator = SessionAuthenticator(
            getAccessToken = { SessionHolder.instance.getAccessToken() },
            getRefreshToken = { SessionHolder.instance.getRefreshToken() },
            tokenRefresher = Auth0TokenRefresher(),
            onTokenRefreshed = { newAccessToken -> SessionHolder.instance.saveAccessToken(newAccessToken) },
            onSessionExpired = ::forceLogout,
        )

        ApiClient.init(
            BuildConfig.API_BASE_URL,
            SessionHolder.instance,
            enableLogging = BuildConfig.DEBUG,
            authenticator = authenticator,
        )

        if (SessionHolder.instance.isLoggedIn()) {
            LocationService.start(this)
        }

        // ProcessLifecycleOwner fires onStop only when every Activity of ours is
        // gone from the foreground (not on in-app navigation between our own
        // screens), so this is the actual "while in use" boundary - unlike
        // MapsActivity's own onStop, which fires on navigating to e.g. Profile
        // too and previously left the foreground service running regardless.
        ProcessLifecycleOwner.get().lifecycle.addObserver(object : DefaultLifecycleObserver {
            override fun onStart(owner: LifecycleOwner) {
                if (SessionHolder.instance.isLoggedIn()) {
                    LocationService.start(this@BubuTrackerApp)
                }
            }

            override fun onStop(owner: LifecycleOwner) {
                LocationService.stop(this@BubuTrackerApp)
            }
        })
    }

    // May run off the main thread with no foreground Activity — start a fresh task.
    private fun forceLogout() {
        LocationService.stop(this)
        SessionHolder.instance.clear()
        UserProfileStore.clear()
        TrackedLocationsStore.clear()
        val intent = Intent(AppActions.ACTION_LOGIN)
            .setPackage(packageName)
            .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK)
        startActivity(intent)
    }
}
