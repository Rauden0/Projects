package com.example.bubutracker

import android.widget.EditText
import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.action.ViewActions.click
import androidx.test.espresso.action.ViewActions.replaceText
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withClassName
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.matcher.ViewMatchers.withText
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TokenProvider
import com.example.bubutracker.core.session.SessionHolder
import com.example.bubutracker.feature.auth.LoginActivity
import com.example.bubutracker.feature.auth.R as AuthR
import com.example.bubutracker.feature.map.R as MapR
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.hamcrest.Matchers.equalTo
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import java.util.concurrent.TimeUnit

@RunWith(AndroidJUnit4::class)
class EndToEndFlowTest {
    private lateinit var server: MockWebServer

    @Before
    fun setUp() {
        val context = InstrumentationRegistry.getInstrumentation().targetContext
        SessionHolder.init(context)
        SessionHolder.instance.saveAccessToken("e2e-token")

        server = MockWebServer()
        server.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse = when (request.path) {
                "/api/v1/users/me" -> MockResponse().setResponseCode(200)
                    .setBody("""{"id":"u1","email":"jane@example.com","firstName":"Jane","lastName":"Doe"}""")
                "/api/v1/locations/tracked" -> MockResponse().setResponseCode(200).setBody("[]")
                "/api/v1/tracking" -> MockResponse().setResponseCode(200)
                else -> MockResponse().setResponseCode(404)
            }
        }
        server.start()
        ApiClient.init(server.url("/").toString(), TokenProvider { "e2e-token" })
    }

    @After
    fun tearDown() {
        SessionHolder.instance.clear()
        server.shutdown()
    }

    @Test
    fun loginRedirectsToMapAddsTrackingThenLogsOutBackToLogin() {
        ActivityScenario.launch(LoginActivity::class.java).use {
            onView(withId(MapR.id.addTrackingButton)).check(matches(isDisplayed()))

            onView(withId(MapR.id.addTrackingButton)).perform(click())
            onView(withClassName(equalTo(EditText::class.java.name)))
                .perform(replaceText("tracked@example.com"))
            onView(withText(MapR.string.add)).perform(click())

            var recorded = server.takeRequest(5, TimeUnit.SECONDS)
            while (recorded != null && recorded.path != "/api/v1/tracking") {
                recorded = server.takeRequest(5, TimeUnit.SECONDS)
            }
            assertNotNull("expected an addTracking request to reach the server", recorded)
            assertEquals("POST", recorded!!.method)

            onView(withId(MapR.id.logoutButton)).perform(click())

            onView(withId(AuthR.id.loginButton)).check(matches(isDisplayed()))
        }
    }
}
