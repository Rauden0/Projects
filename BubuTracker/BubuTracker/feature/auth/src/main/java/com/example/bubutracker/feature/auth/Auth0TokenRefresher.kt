package com.example.bubutracker.feature.auth

import com.auth0.android.Auth0
import com.auth0.android.Auth0Exception
import com.auth0.android.authentication.AuthenticationAPIClient
import com.example.bubutracker.core.network.TokenRefresher

/** Blocking refresh for OkHttp's synchronous Authenticator. */
class Auth0TokenRefresher(
    clientId: String = BuildConfig.AUTH0_CLIENT_ID,
    domain: String = BuildConfig.AUTH0_DOMAIN,
) : TokenRefresher {
    private val client = AuthenticationAPIClient(Auth0.getInstance(clientId, domain))

    override fun refresh(refreshToken: String): String? =
        try {
            client.renewAuth(refreshToken).execute().accessToken
        } catch (_: Auth0Exception) {
            null
        }
}
