package com.example.bubutracker.core.navigation

import android.app.Activity
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.Robolectric
import org.robolectric.RobolectricTestRunner
import org.robolectric.Shadows.shadowOf

@RunWith(RobolectricTestRunner::class)
class NavigatorTest {

    @Test
    fun navigateToStartsIntentWithActionScopedToOwnPackage() {
        val activity = Robolectric.buildActivity(Activity::class.java).create().get()

        activity.navigateTo("com.example.test.ACTION_FOO")

        val started = shadowOf(activity).nextStartedActivity
        assertEquals("com.example.test.ACTION_FOO", started.action)
        assertEquals(activity.packageName, started.`package`)
    }

    @Test
    fun navigateToFinishesCallingActivityWhenRequested() {
        val activity = Robolectric.buildActivity(Activity::class.java).create().get()

        activity.navigateTo("com.example.test.ACTION_FOO", finishCurrent = true)

        assertTrue(activity.isFinishing)
    }

    @Test
    fun navigateToDoesNotFinishByDefault() {
        val activity = Robolectric.buildActivity(Activity::class.java).create().get()

        activity.navigateTo("com.example.test.ACTION_FOO")

        assertFalse(activity.isFinishing)
    }
}
