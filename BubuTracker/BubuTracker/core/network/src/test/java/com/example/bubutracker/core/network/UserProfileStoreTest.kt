package com.example.bubutracker.core.network

import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Before
import org.junit.Test

class UserProfileStoreTest {

    private val profile = UserProfileDto(
        id = "u1",
        email = "alice@example.com",
        firstName = "Alice",
        lastName = "Smith",
        markerColor = "#FF9800"
    )

    @Before
    @After
    fun resetStore() {
        // The store is a process-wide singleton; isolate each test from the
        // others' writes rather than assuming execution order.
        UserProfileStore.clear()
    }

    @Test
    fun `getFresh returns null when nothing has been cached yet`() {
        assertNull(UserProfileStore.getFresh())
    }

    @Test
    fun `getFresh returns the cached profile shortly after set`() {
        UserProfileStore.set(profile)

        assertEquals(profile, UserProfileStore.getFresh())
    }

    @Test
    fun `getFresh returns null once the soft TTL has elapsed`() {
        val now = System.currentTimeMillis()
        UserProfileStore.set(profile)

        assertEquals(profile, UserProfileStore.getFresh(nowMillis = now + 4 * 60_000L))
        assertNull(UserProfileStore.getFresh(nowMillis = now + 6 * 60_000L))
    }

    @Test
    fun `clear drops the cached profile immediately`() {
        UserProfileStore.set(profile)
        UserProfileStore.clear()

        assertNull(UserProfileStore.getFresh())
    }

    @Test
    fun `set overwrites a previous entry and resets its freshness`() {
        val stale = System.currentTimeMillis()
        UserProfileStore.set(profile)

        val updated = profile.copy(firstName = "Alicia")
        UserProfileStore.set(updated)

        assertEquals(updated, UserProfileStore.getFresh(nowMillis = stale + 4 * 60_000L))
    }
}
