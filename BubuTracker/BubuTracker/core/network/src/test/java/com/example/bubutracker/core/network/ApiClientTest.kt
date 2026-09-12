package com.example.bubutracker.core.network

import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class ApiClientTest {
    private lateinit var server: MockWebServer

    @Before
    fun setUp() {
        server = MockWebServer()
        server.start()
        ApiClient.init(server.url("/").toString(), TokenProvider { "test-token" })
    }

    @After
    fun tearDown() {
        server.shutdown()
    }

    @Test
    fun getMeDeserializesResponseAndSendsAuthHeader() {
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody("""{"id":"u1","email":"jane@example.com","firstName":"Jane","lastName":"Doe"}""")
        )

        val response = ApiClient.service.getMe().execute()

        assertTrue(response.isSuccessful)
        assertEquals("jane@example.com", response.body()?.email)

        val recorded = server.takeRequest()
        assertEquals("Bearer test-token", recorded.getHeader("Authorization"))
        assertEquals("/api/v1/users/me", recorded.path)
    }

    @Test
    fun addTrackingPostsExpectedBody() {
        server.enqueue(MockResponse().setResponseCode(200))

        ApiClient.service.addTracking(AddTrackingDto("tracked@example.com")).execute()

        val recorded = server.takeRequest()
        assertEquals("POST", recorded.method)
        assertEquals("/api/v1/tracking", recorded.path)
        assertTrue(recorded.body.readUtf8().contains("tracked@example.com"))
    }

    @Test
    fun reinitializingPointsAtTheNewServer() {
        val secondServer = MockWebServer()
        secondServer.start()
        try {
            secondServer.enqueue(MockResponse().setResponseCode(204))
            ApiClient.init(secondServer.url("/").toString(), TokenProvider { "other-token" })

            ApiClient.service.getMe().execute()

            assertEquals("Bearer other-token", secondServer.takeRequest().getHeader("Authorization"))
            assertEquals(0, server.requestCount)
        } finally {
            secondServer.shutdown()
        }
    }
}
