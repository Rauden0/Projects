package com.example.bubutracker.feature.profile

import android.app.Instrumentation
import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.action.ViewActions.click
import androidx.test.espresso.action.ViewActions.replaceText
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.intent.Intents
import androidx.test.espresso.intent.Intents.intended
import androidx.test.espresso.intent.matcher.IntentMatchers.hasAction
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.matcher.ViewMatchers.withText
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TokenProvider
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class ProfileActivityTest {
    private lateinit var server: MockWebServer

    @Before
    fun setUp() {
        server = MockWebServer()
        server.start()
        ApiClient.init(server.url("/").toString(), TokenProvider { "token" })
        Intents.init()
    }

    @After
    fun tearDown() {
        Intents.release()
        server.shutdown()
    }

    @Test
    fun loadsAndDisplaysExistingProfile() {
        server.enqueue(
            MockResponse().setResponseCode(200)
                .setBody("""{"id":"u1","email":"jane@example.com","firstName":"Jane","lastName":"Doe"}""")
        )

        ActivityScenario.launch(ProfileActivity::class.java).use {
            waitFor { onView(withId(R.id.FirstNameTextInput)).check(matches(withText("Jane"))) }
            onView(withId(R.id.LastNameTextInput)).check(matches(withText("Doe")))
        }
    }

    @Test
    fun savingProfileNavigatesToMap() {
        server.enqueue(
            MockResponse().setResponseCode(200)
                .setBody("""{"id":"u1","email":"jane@example.com","firstName":"","lastName":""}""")
        )
        server.enqueue(
            MockResponse().setResponseCode(200)
                .setBody("""{"id":"u1","email":"jane@example.com","firstName":"Jane","lastName":"Doe"}""")
        )
        Intents.intending(hasAction(AppActions.ACTION_MAP))
            .respondWith(Instrumentation.ActivityResult(0, null))

        ActivityScenario.launch(ProfileActivity::class.java).use {
            onView(withId(R.id.FirstNameTextInput)).perform(replaceText("Jane"))
            onView(withId(R.id.LastNameTextInput)).perform(replaceText("Doe"))
            onView(withId(R.id.saveProfileButton)).perform(click())

            waitFor { intended(hasAction(AppActions.ACTION_MAP)) }
        }
    }

    // Poll until assertion passes — no IdlingResource wired for Retrofit callbacks.
    private fun waitFor(timeoutMs: Long = 5_000, intervalMs: Long = 100, assertion: () -> Unit) {
        val deadline = System.currentTimeMillis() + timeoutMs
        var lastError: Throwable? = null
        while (System.currentTimeMillis() < deadline) {
            try {
                assertion()
                return
            } catch (t: Throwable) {
                lastError = t
                Thread.sleep(intervalMs)
            }
        }
        throw lastError ?: AssertionError("waitFor timed out")
    }
}
