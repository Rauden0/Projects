package com.example.bubutracker.feature.auth

import com.auth0.android.Auth0
import com.auth0.android.Auth0Exception
import com.auth0.android.authentication.AuthenticationAPIClient
import com.example.bubutracker.core.network.TokenRefresher

/**
 * Exchanges the stored Auth0 refresh token for a new access token via the
 * `refresh_token` grant. [refresh] runs synchronously (blocking) because it's called
 * from [com.example.bubutracker.core.network.SessionAuthenticator], which OkHttp
 * invokes synchronously on the thread that received the 401.
 *
 * Requires the initial login to have requested the `offline_access` scope (see
 * [LoginActivity]) and the Auth0 API to have "Allow Offline Access" enabled -
 * otherwise Auth0 never issues a refresh token and this is never reached.
 */
class Auth0TokenRefresher(
    clientId: String = BuildConfig.AUTH0_CLIENT_ID,
    domain: String = BuildConfig.AUTH0_DOMAIN,
) : TokenRefresher {
    private val client = AuthenticationAPIClient(Auth0.getInstance(clientId, domain))

    override fun refresh(refreshToken: String): String? =
        try {
            client.renewAuth(refreshToken).execute().accessToken
        } catch (_: Auth0Exception) {
            // Expired/revoked refresh token, network failure, etc. - any failure here
            // safely degrades to SessionAuthenticator forcing a fresh login.
            null
        }
}
