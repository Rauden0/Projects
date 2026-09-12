package com.example.bubutracker.core.network

import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Before
import org.junit.Test

class AuthInterceptorTest {
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

    private fun callWith(tokenProvider: TokenProvider) {
        server.enqueue(MockResponse().setResponseCode(200))
        val client = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(tokenProvider))
            .build()
        client.newCall(Request.Builder().url(server.url("/ping")).build()).execute().close()
    }

    @Test
    fun addsBearerHeaderWhenTokenPresent() {
        callWith(TokenProvider { "abc123" })

        assertEquals("Bearer abc123", server.takeRequest().getHeader("Authorization"))
    }

    @Test
    fun omitsHeaderWhenTokenIsNull() {
        callWith(TokenProvider { null })

        assertNull(server.takeRequest().getHeader("Authorization"))
    }

    @Test
    fun omitsHeaderWhenTokenIsBlank() {
        callWith(TokenProvider { "   " })

        assertNull(server.takeRequest().getHeader("Authorization"))
    }
}
