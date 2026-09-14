package com.example.bubutracker.core.network

import okhttp3.OkHttpClient
import okhttp3.Protocol
import okhttp3.Request
import okhttp3.Response
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class SessionAuthenticatorTest {
    private lateinit var server: MockWebServer

    @Before
    fun setUp() {
        server = MockWebServer()
        server.start()
    }

    @After
    fun tearDown() {
        server.shutdown()
    }

    private fun clientWith(authenticator: SessionAuthenticator) = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor(TokenProvider { "stale-token" }))
        .authenticator(authenticator)
        .build()

    @Test
    fun refreshesAndRetriesOn401() {
        server.enqueue(MockResponse().setResponseCode(401))
        server.enqueue(MockResponse().setResponseCode(200))

        var savedToken: String? = null
        var sessionExpired = false
        val authenticator = SessionAuthenticator(
            getRefreshToken = { "refresh-token" },
            tokenRefresher = TokenRefresher { "new-access-token" },
            onTokenRefreshed = { savedToken = it },
            onSessionExpired = { sessionExpired = true },
        )

        val response = clientWith(authenticator)
            .newCall(Request.Builder().url(server.url("/ping")).build())
            .execute()
        response.close()

        assertTrue(response.isSuccessful)
        assertEquals("new-access-token", savedToken)
        assertFalse(sessionExpired)

        server.takeRequest() // the original, stale-token request
        val retried = server.takeRequest()
        assertEquals("Bearer new-access-token", retried.getHeader("Authorization"))
    }

    @Test
    fun reportsSessionExpiredWhenNoRefreshTokenStored() {
        server.enqueue(MockResponse().setResponseCode(401))

        var sessionExpired = false
        val authenticator = SessionAuthenticator(
            getRefreshToken = { null },
            tokenRefresher = TokenRefresher { "should-not-be-called" },
            onTokenRefreshed = { },
            onSessionExpired = { sessionExpired = true },
        )

        val response = clientWith(authenticator)
            .newCall(Request.Builder().url(server.url("/ping")).build())
            .execute()
        response.close()

        assertEquals(401, response.code)
        assertTrue(sessionExpired)
    }

    @Test
    fun reportsSessionExpiredWhenRefreshCallFails() {
        server.enqueue(MockResponse().setResponseCode(401))

        var sessionExpired = false
        val authenticator = SessionAuthenticator(
            getRefreshToken = { "refresh-token" },
            tokenRefresher = TokenRefresher { null },
            onTokenRefreshed = { },
            onSessionExpired = { sessionExpired = true },
        )

        val response = clientWith(authenticator)
            .newCall(Request.Builder().url(server.url("/ping")).build())
            .execute()
        response.close()

        assertEquals(401, response.code)
        assertTrue(sessionExpired)
    }

    @Test
    fun givesUpAfterOneRetryInsteadOfLoopingForever() {
        server.enqueue(MockResponse().setResponseCode(401))
        server.enqueue(MockResponse().setResponseCode(401))

        var refreshCalls = 0
        var sessionExpired = false
        val authenticator = SessionAuthenticator(
            getRefreshToken = { "refresh-token" },
            tokenRefresher = TokenRefresher { refreshCalls++; "new-access-token" },
            onTokenRefreshed = { },
            onSessionExpired = { sessionExpired = true },
        )

        val response = clientWith(authenticator)
            .newCall(Request.Builder().url(server.url("/ping")).build())
            .execute()
        response.close()

        assertEquals(401, response.code)
        assertEquals(1, refreshCalls)
        assertTrue(sessionExpired)
    }

    @Test
    fun reusesAlreadyRefreshedTokenInsteadOfRefreshingAgain() {
        // Simulates losing the race to a concurrent authenticate() call that
        // already refreshed the token: getAccessToken() returns something
        // other than the token this particular request failed with.
        var refreshCalls = 0
        var tokenRefreshedCalls = 0
        val authenticator = SessionAuthenticator(
            getAccessToken = { "already-refreshed-token" },
            getRefreshToken = { "refresh-token" },
            tokenRefresher = TokenRefresher { refreshCalls++; "should-not-be-used" },
            onTokenRefreshed = { tokenRefreshedCalls++ },
            onSessionExpired = { },
        )

        val failedResponse = Response.Builder()
            .request(
                Request.Builder()
                    .url("http://localhost/ping")
                    .header("Authorization", "Bearer stale-token")
                    .build(),
            )
            .protocol(Protocol.HTTP_1_1)
            .code(401)
            .message("Unauthorized")
            .build()

        val retried = authenticator.authenticate(null, failedResponse)

        assertEquals("Bearer already-refreshed-token", retried?.header("Authorization"))
        assertEquals(0, refreshCalls)
        assertEquals(0, tokenRefreshedCalls)
    }
}
