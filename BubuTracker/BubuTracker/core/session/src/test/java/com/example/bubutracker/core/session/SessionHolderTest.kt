package com.example.bubutracker.core.session

import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotSame
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment

@RunWith(RobolectricTestRunner::class)
class SessionHolderTest {

    @Before
    fun setUp() {
        SessionHolder.init(RuntimeEnvironment.getApplication())
        SessionHolder.instance.clear()
    }

    @Test
    fun initExposesAWorkingSessionManager() {
        assertFalse(SessionHolder.instance.isLoggedIn())

        SessionHolder.instance.saveAccessToken("token")

        assertTrue(SessionHolder.instance.isLoggedIn())
    }

    @Test
    fun reinitializingReplacesTheInstance() {
        val first = SessionHolder.instance
        SessionHolder.init(RuntimeEnvironment.getApplication())

        assertNotSame(first, SessionHolder.instance)
    }
}
