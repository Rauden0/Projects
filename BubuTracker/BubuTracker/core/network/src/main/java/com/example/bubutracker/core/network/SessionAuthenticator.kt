package com.example.bubutracker.core.network

import okhttp3.Authenticator
import okhttp3.Request
import okhttp3.Response
import okhttp3.Route

/**
 * Exchanges a refresh token for a new access token. Implemented by a module that
 * has an identity-provider SDK on its classpath (e.g. `:feature:auth` via Auth0).
 * Must be a blocking call: [SessionAuthenticator] is invoked synchronously by OkHttp
 * on the thread that received the 401, per the [Authenticator] contract.
 */
fun interface TokenRefresher {
    /** Returns the new access token, or null if the refresh itself failed. */
    fun refresh(refreshToken: String): String?
}

/**
 * Recovers from an expired/invalid access token by refreshing it once and retrying
 * the request, instead of every call just failing with 401 forever once the token
 * expires. Falls back to [onSessionExpired] when there's no refresh token, the
 * refresh call fails, or the retried request still comes back unauthorized.
 */
class SessionAuthenticator(
    private val getRefreshToken: () -> String?,
    private val tokenRefresher: TokenRefresher,
    private val onTokenRefreshed: (String) -> Unit,
    private val onSessionExpired: () -> Unit,
) : Authenticator {
    override fun authenticate(route: Route?, response: Response): Request? {
        if (priorResponseCount(response) >= MAX_ATTEMPTS) {
            // Already retried once with a freshly refreshed token and still got a
            // 401 back - the new token is bad too, so stop instead of looping.
            onSessionExpired()
            return null
        }

        val refreshToken = getRefreshToken()
        if (refreshToken.isNullOrBlank()) {
            onSessionExpired()
            return null
        }

        val newAccessToken = tokenRefresher.refresh(refreshToken)
        if (newAccessToken.isNullOrBlank()) {
            onSessionExpired()
            return null
        }

        onTokenRefreshed(newAccessToken)
        return response.request.newBuilder()
            .header("Authorization", "Bearer $newAccessToken")
            .build()
    }

    private fun priorResponseCount(response: Response): Int {
        var count = 1
        var prior = response.priorResponse
        while (prior != null) {
            count++
            prior = prior.priorResponse
        }
        return count
    }

    private companion object {
        const val MAX_ATTEMPTS = 2
    }
}
