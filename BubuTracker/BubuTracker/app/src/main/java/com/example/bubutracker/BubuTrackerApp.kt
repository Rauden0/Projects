package com.example.bubutracker

import android.app.Application
import android.content.Intent
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.SessionAuthenticator
import com.example.bubutracker.core.session.SessionHolder
import com.example.bubutracker.feature.auth.Auth0TokenRefresher

class BubuTrackerApp : Application() {
    override fun onCreate() {
        super.onCreate()
        SessionHolder.init(this)

        val authenticator = SessionAuthenticator(
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
    }

    // Called off the main thread (SessionAuthenticator runs on OkHttp's dispatcher),
    // and there's no guaranteed foreground Activity to route from, so this starts
    // LoginActivity directly from the Application context with a fresh task instead
    // of relying on core.navigation.navigateTo's Activity-scoped finish() behavior.
    private fun forceLogout() {
        SessionHolder.instance.clear()
        val intent = Intent(AppActions.ACTION_LOGIN)
            .setPackage(packageName)
            .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK)
        startActivity(intent)
    }
}
