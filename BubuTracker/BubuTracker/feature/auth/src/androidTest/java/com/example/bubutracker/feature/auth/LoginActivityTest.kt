package com.example.bubutracker.feature.auth

import android.app.Instrumentation
import androidx.test.core.app.ActivityScenario
import androidx.test.espresso.Espresso.onView
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.intent.Intents
import androidx.test.espresso.intent.Intents.intended
import androidx.test.espresso.intent.matcher.IntentMatchers.hasAction
import androidx.test.espresso.intent.matcher.IntentMatchers.hasPackage
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.matcher.ViewMatchers.withText
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import com.example.bubutracker.core.designsystem.R as DesignSystemR
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.session.SessionHolder
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class LoginActivityTest {

    @Before
    fun setUp() {
        val context = InstrumentationRegistry.getInstrumentation().targetContext
        SessionHolder.init(context)
        SessionHolder.instance.clear()
        Intents.init()
    }

    @After
    fun tearDown() {
        SessionHolder.instance.clear()
        Intents.release()
    }

    @Test
    fun showsLoginUiWhenLoggedOut() {
        ActivityScenario.launch(LoginActivity::class.java).use {
            onView(withId(R.id.loginButton)).check(matches(isDisplayed()))
            onView(withId(R.id.loginTitle)).check(matches(withText(DesignSystemR.string.bubu_tracker)))
            onView(withId(R.id.loginSubtitle)).check(matches(withText(R.string.login_subtitle)))
        }
    }

    @Test
    fun redirectsToMapWhenAlreadyLoggedIn() {
        SessionHolder.instance.saveAccessToken("existing-token")
        val targetPackage = InstrumentationRegistry.getInstrumentation().targetContext.packageName
        Intents.intending(hasAction(AppActions.ACTION_MAP))
            .respondWith(Instrumentation.ActivityResult(0, null))

        ActivityScenario.launch(LoginActivity::class.java).use {
            intended(hasAction(AppActions.ACTION_MAP))
            intended(hasPackage(targetPackage))
        }
    }
}
