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

/**
 * feature:map doesn't depend on feature:auth, so the LOGIN target doesn't exist in this
 * module's own test APK; Espresso-Intents stubs it (see feature:auth's LoginActivityTest
 * for the same rationale). The backend is stood in for by an in-process MockWebServer.
 */
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
