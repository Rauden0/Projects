package com.example.bubutracker.core.session

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
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

    @Test
    fun getRefreshToken_returnsNull_whenNoneSaved() {
        assertNull(sessionManager.getRefreshToken())
    }

    @Test
    fun saveCredentials_persistsBothTokens() {
        sessionManager.saveCredentials("access-1", "refresh-1")

        assertEquals("access-1", sessionManager.getAccessToken())
        assertEquals("refresh-1", sessionManager.getRefreshToken())
    }

    @Test
    fun saveCredentials_withNullRefreshToken_leavesRefreshTokenUnset() {
        sessionManager.saveCredentials("access-1", null)

        assertNull(sessionManager.getRefreshToken())
    }

    @Test
    fun clear_removesRefreshTokenToo() {
        sessionManager.saveCredentials("access-1", "refresh-1")
        sessionManager.clear()

        assertNull(sessionManager.getRefreshToken())
    }
}
