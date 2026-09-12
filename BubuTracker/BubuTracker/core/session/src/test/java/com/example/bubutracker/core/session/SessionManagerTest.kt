package com.example.bubutracker.core.session

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment

@RunWith(RobolectricTestRunner::class)
class SessionManagerTest {
    private lateinit var sessionManager: SessionManager

    @Before
    fun setUp() {
        val context = RuntimeEnvironment.getApplication()
        sessionManager = SessionManager(context)
        sessionManager.clear()
    }

    @Test
    fun isLoggedIn_returnsFalse_whenNoToken() {
        assertFalse(sessionManager.isLoggedIn())
    }

    @Test
    fun isLoggedIn_returnsTrue_afterSavingToken() {
        sessionManager.saveAccessToken("test-token")
        assertTrue(sessionManager.isLoggedIn())
    }

    @Test
    fun clear_removesSavedToken() {
        sessionManager.saveAccessToken("test-token")
        sessionManager.clear()
        assertFalse(sessionManager.isLoggedIn())
    }
}
