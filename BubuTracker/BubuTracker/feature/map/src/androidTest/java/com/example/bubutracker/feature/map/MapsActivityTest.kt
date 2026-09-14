package com.example.bubutracker.feature.map

import android.app.Instrumentation
import android.widget.EditText
import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.action.ViewActions.click
import androidx.test.espresso.action.ViewActions.replaceText
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.intent.Intents
import androidx.test.espresso.intent.Intents.intended
import androidx.test.espresso.intent.matcher.IntentMatchers.hasAction
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withClassName
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.matcher.ViewMatchers.withText
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TokenProvider
import com.example.bubutracker.core.session.SessionHolder
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.hamcrest.Matchers.equalTo
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import java.util.concurrent.TimeUnit

@RunWith(AndroidJUnit4::class)
class MapsActivityTest {
    private lateinit var server: MockWebServer

    @Before
    fun setUp() {
        val context = InstrumentationRegistry.getInstrumentation().targetContext
        SessionHolder.init(context)

        server = MockWebServer()
        server.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse = when (request.path) {
                "/api/v1/locations/tracked" -> MockResponse().setResponseCode(200).setBody("[]")
                "/api/v1/tracking" -> MockResponse().setResponseCode(200)
                else -> MockResponse().setResponseCode(404)
            }
        }
        server.start()
        ApiClient.init(server.url("/").toString(), TokenProvider { "token" })

        Intents.init()
    }

    @After
    fun tearDown() {
        Intents.release()
        server.shutdown()
        SessionHolder.instance.clear()
    }

    @Test
    fun redirectsToLoginWhenLoggedOut() {
        SessionHolder.instance.clear()
        Intents.intending(hasAction(AppActions.ACTION_LOGIN))
            .respondWith(Instrumentation.ActivityResult(0, null))

        ActivityScenario.launch(MapsActivity::class.java).use {
            intended(hasAction(AppActions.ACTION_LOGIN))
        }
    }

    @Test
    fun showsMapControlsWhenLoggedIn() {
        SessionHolder.instance.saveAccessToken("token")

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.addTrackingButton)).check(matches(isDisplayed()))
            onView(withId(R.id.logoutButton)).check(matches(isDisplayed()))
            onView(withId(R.id.trackingRequestsButton)).check(matches(isDisplayed()))
            onView(withId(R.id.followersButton)).check(matches(isDisplayed()))
            onView(withId(R.id.peopleITrackButton)).check(matches(isDisplayed()))
        }
    }

    @Test
    fun addTrackingDialogSubmitsEmailToApi() {
        SessionHolder.instance.saveAccessToken("token")

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.addTrackingButton)).perform(click())
            onView(withClassName(equalTo(EditText::class.java.name)))
                .perform(replaceText("tracked@example.com"))
            onView(withText(R.string.add)).perform(click())
        }

        var recorded = server.takeRequest(5, TimeUnit.SECONDS)
        while (recorded != null && recorded.path != "/api/v1/tracking") {
            recorded = server.takeRequest(5, TimeUnit.SECONDS)
        }
        assertNotNull("expected an addTracking request to reach the server", recorded)
        assertEquals("POST", recorded!!.method)
        assertTrue(recorded.body.readUtf8().contains("tracked@example.com"))
    }

    @Test
    fun requestsDialogAcceptsPendingRequest() {
        SessionHolder.instance.saveAccessToken("token")
        val trackerId = "11111111-1111-1111-1111-111111111111"
        server.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse = when (request.path) {
                "/api/v1/tracking/requests" -> MockResponse().setResponseCode(200)
                    .setBody("""[{"id":"$trackerId","email":"wannabe@example.com","firstName":"Wanda","lastName":"B"}]""")
                "/api/v1/tracking/requests/$trackerId/accept" -> MockResponse().setResponseCode(204)
                "/api/v1/locations/tracked" -> MockResponse().setResponseCode(200).setBody("[]")
                else -> MockResponse().setResponseCode(404)
            }
        }

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.trackingRequestsButton)).perform(click())
            onView(withText("Wanda B")).perform(click())
            onView(withText(R.string.accept)).perform(click())
        }

        var recorded = server.takeRequest(5, TimeUnit.SECONDS)
        while (recorded != null && recorded.path != "/api/v1/tracking/requests/$trackerId/accept") {
            recorded = server.takeRequest(5, TimeUnit.SECONDS)
        }
        assertNotNull("expected an accept request to reach the server", recorded)
        assertEquals("POST", recorded!!.method)
    }

    @Test
    fun followersDialogRevokesAFollower() {
        SessionHolder.instance.saveAccessToken("token")
        val trackerId = "22222222-2222-2222-2222-222222222222"
        server.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse = when {
                request.path == "/api/v1/tracking/followers" -> MockResponse().setResponseCode(200)
                    .setBody("""[{"id":"$trackerId","email":"follower@example.com","firstName":"Fred","lastName":"O"}]""")
                request.path == "/api/v1/tracking/followers/$trackerId" && request.method == "DELETE" ->
                    MockResponse().setResponseCode(204)
                request.path == "/api/v1/locations/tracked" -> MockResponse().setResponseCode(200).setBody("[]")
                else -> MockResponse().setResponseCode(404)
            }
        }

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.followersButton)).perform(click())
            onView(withText("Fred O")).perform(click())
            onView(withText(R.string.revoke)).perform(click())
        }

        var recorded = server.takeRequest(5, TimeUnit.SECONDS)
        while (recorded != null && recorded.path != "/api/v1/tracking/followers/$trackerId") {
            recorded = server.takeRequest(5, TimeUnit.SECONDS)
        }
        assertNotNull("expected a revoke request to reach the server", recorded)
        assertEquals("DELETE", recorded!!.method)
    }

    @Test
    fun peopleITrackDialogStopsTracking() {
        SessionHolder.instance.saveAccessToken("token")
        val trackedId = "33333333-3333-3333-3333-333333333333"
        server.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse = when {
                request.path == "/api/v1/tracking" && request.method == "GET" -> MockResponse().setResponseCode(200)
                    .setBody("""[{"id":"$trackedId","email":"tracked@example.com","firstName":"Tina","lastName":"K"}]""")
                request.path == "/api/v1/tracking/$trackedId" && request.method == "DELETE" ->
                    MockResponse().setResponseCode(204)
                request.path == "/api/v1/locations/tracked" -> MockResponse().setResponseCode(200).setBody("[]")
                else -> MockResponse().setResponseCode(404)
            }
        }

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.peopleITrackButton)).perform(click())
            onView(withText("Tina K")).perform(click())
            onView(withText(R.string.stop_tracking)).perform(click())
        }

        var recorded = server.takeRequest(5, TimeUnit.SECONDS)
        while (recorded != null && recorded.path != "/api/v1/tracking/$trackedId") {
            recorded = server.takeRequest(5, TimeUnit.SECONDS)
        }
        assertNotNull("expected a stop-tracking request to reach the server", recorded)
        assertEquals("DELETE", recorded!!.method)
    }

    @Test
    fun logoutRedirectsToLogin() {
        SessionHolder.instance.saveAccessToken("token")
        Intents.intending(hasAction(AppActions.ACTION_LOGIN))
            .respondWith(Instrumentation.ActivityResult(0, null))

        ActivityScenario.launch(MapsActivity::class.java).use {
            onView(withId(R.id.logoutButton)).perform(click())
            intended(hasAction(AppActions.ACTION_LOGIN))
        }
    }
}
