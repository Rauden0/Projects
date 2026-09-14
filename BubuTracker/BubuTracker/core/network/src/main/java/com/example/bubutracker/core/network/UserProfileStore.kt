package com.example.bubutracker.core.network

/**
 * Process-scoped single source of truth for the signed-in user's own profile,
 * so LoginActivity, MapsActivity, and ProfileActivity don't each independently
 * hit `GET /users/me` when one of them just fetched it moments ago.
 *
 * Callers are expected to write the result of every successful getMe/updateMe
 * response via [set], and clear it on logout / account deletion via [clear].
 * [getFresh] is a soft cache: it returns null once [SOFT_TTL_MS] has elapsed
 * so a caller falls back to a real fetch rather than serving indefinitely
 * stale data if nothing happened to invalidate it in the meantime.
 */
object UserProfileStore {
    @Volatile
    private var cached: UserProfileDto? = null
    @Volatile
    private var cachedAtMillis: Long = 0L

    // nowMillis is a parameter (defaulting to real time) rather than read
    // internally so a test can pin it instead of sleeping past the TTL.
    fun getFresh(nowMillis: Long = System.currentTimeMillis()): UserProfileDto? {
        val profile = cached ?: return null
        return if (nowMillis - cachedAtMillis < SOFT_TTL_MS) profile else null
    }

    fun set(profile: UserProfileDto) {
        cached = profile
        cachedAtMillis = System.currentTimeMillis()
    }

    fun clear() {
        cached = null
        cachedAtMillis = 0L
    }

    private const val SOFT_TTL_MS = 5 * 60_000L
}
