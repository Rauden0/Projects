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
    fun getIncomingTrackingRequestsHitsTheRequestsEndpoint() {
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody("""[{"id":"u2","email":"wannabe@example.com","firstName":"","lastName":""}]""")
        )

        val response = ApiClient.service.getIncomingTrackingRequests().execute()

        assertTrue(response.isSuccessful)
        assertEquals("wannabe@example.com", response.body()?.single()?.email)
        assertEquals("/api/v1/tracking/requests", server.takeRequest().path)
    }

    @Test
    fun acceptTrackingRequestPostsToTheAcceptEndpoint() {
        server.enqueue(MockResponse().setResponseCode(204))

        ApiClient.service.acceptTrackingRequest("tracker-id").execute()

        val recorded = server.takeRequest()
        assertEquals("POST", recorded.method)
        assertEquals("/api/v1/tracking/requests/tracker-id/accept", recorded.path)
    }

    @Test
    fun getFollowersHitsTheFollowersEndpoint() {
        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody("""[{"id":"u3","email":"follower@example.com","firstName":"","lastName":""}]""")
        )

        val response = ApiClient.service.getFollowers().execute()

        assertTrue(response.isSuccessful)
        assertEquals("follower@example.com", response.body()?.single()?.email)
        assertEquals("/api/v1/tracking/followers", server.takeRequest().path)
    }

    @Test
    fun removeFollowerDeletesTheFollowerEndpoint() {
        server.enqueue(MockResponse().setResponseCode(204))

        ApiClient.service.removeFollower("tracker-id").execute()

        val recorded = server.takeRequest()
        assertEquals("DELETE", recorded.method)
        assertEquals("/api/v1/tracking/followers/tracker-id", recorded.path)
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
