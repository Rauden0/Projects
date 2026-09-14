package com.example.bubutracker.core.network

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken

/**
 * Disk-backed snapshot of the last successfully fetched tracked-locations
 * list, so a cold start can paint markers immediately instead of a blank map
 * while the first network poll is still in flight. Leaving in-memory state
 * untouched on a failed poll already covers "poll failed while the app was
 * running" for free; this store exists specifically for the "process was
 * killed, just relaunched" case, where there is no in-memory state at all.
 */
object TrackedLocationsStore {
    private val gson = Gson()
    private val listType = object : TypeToken<List<TrackedLocationDto>>() {}.type

    @Volatile
    private var prefs: SharedPreferences? = null

    fun init(context: Context) {
        prefs = context.applicationContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
    }

    fun getCached(): List<TrackedLocationDto>? {
        val json = prefs?.getString(KEY_SNAPSHOT, null) ?: return null
        return try {
            gson.fromJson<List<TrackedLocationDto>>(json, listType)
        } catch (e: Exception) {
            null
        }
    }

    fun set(locations: List<TrackedLocationDto>) {
        prefs?.edit()?.putString(KEY_SNAPSHOT, gson.toJson(locations))?.apply()
    }

    fun clear() {
        prefs?.edit()?.remove(KEY_SNAPSHOT)?.apply()
    }

    private const val PREFS_NAME = "bubutracker_tracked_locations"
    private const val KEY_SNAPSHOT = "snapshot"
}
