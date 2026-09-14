package com.example.bubutracker.core.network

import okhttp3.Authenticator
import okhttp3.Request
import okhttp3.Response
import okhttp3.Route

/** Blocking refresh — OkHttp invokes Authenticator synchronously on the 401 thread. */
fun interface TokenRefresher {
    fun refresh(refreshToken: String): String?
}

class SessionAuthenticator(
    private val getAccessToken: () -> String? = { null },
    private val getRefreshToken: () -> String?,
    private val tokenRefresher: TokenRefresher,
    private val onTokenRefreshed: (String) -> Unit,
    private val onSessionExpired: () -> Unit,
) : Authenticator {
    // OkHttp can invoke authenticate() concurrently from multiple dispatcher
    // threads (e.g. a location push and a tracked-locations poll both hitting a
    // 401 around the same time). Without this lock both would race to refresh;
    // with refresh-token rotation the loser's request would rotate an
    // already-consumed refresh token and force a spurious logout even though
    // the session was fine.
    private val refreshLock = Any()

    override fun authenticate(route: Route?, response: Response): Request? {
        if (priorResponseCount(response) >= MAX_ATTEMPTS) {
            // Stop after one refresh+retry to avoid an auth loop.
            onSessionExpired()
            return null
        }

        synchronized(refreshLock) {
            val failedToken = response.request.header("Authorization")?.removePrefix("Bearer ")
            val currentToken = getAccessToken()
            if (!currentToken.isNullOrBlank() && currentToken != failedToken) {
                // Another thread already refreshed while we were waiting for the
                // lock - reuse its result instead of refreshing a second time.
                return response.request.newBuilder()
                    .header("Authorization", "Bearer $currentToken")
                    .build()
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
