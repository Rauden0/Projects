package com.example.bubutracker.core.session

import android.content.Context
import android.content.SharedPreferences
import com.example.bubutracker.core.network.TokenProvider

class SessionManager(context: Context) : TokenProvider {
    private val prefs: SharedPreferences =
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    fun saveAccessToken(token: String) {
        prefs.edit().putString(KEY_ACCESS_TOKEN, token).apply()
    }

    override fun getAccessToken(): String? = prefs.getString(KEY_ACCESS_TOKEN, null)

    fun isLoggedIn(): Boolean = !getAccessToken().isNullOrBlank()

    fun clear() {
        prefs.edit().clear().apply()
    }

    companion object {
        private const val PREFS_NAME = "bubutracker_session"
        private const val KEY_ACCESS_TOKEN = "access_token"
    }
}
