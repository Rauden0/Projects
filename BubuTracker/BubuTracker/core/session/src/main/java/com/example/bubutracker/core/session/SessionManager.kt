package com.example.bubutracker.core.session

import android.content.Context
import android.content.SharedPreferences
import android.util.Base64
import com.example.bubutracker.core.network.TokenProvider
import com.google.crypto.tink.Aead
import com.google.crypto.tink.KeyTemplates
import com.google.crypto.tink.aead.AeadConfig
import com.google.crypto.tink.integration.android.AndroidKeysetManager
import java.security.GeneralSecurityException

class SessionManager internal constructor(
    context: Context,
    private val aead: Aead,
) : TokenProvider {
    private val prefs: SharedPreferences =
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    constructor(context: Context) : this(context, buildAead(context))

    fun saveCredentials(accessToken: String, refreshToken: String?) {
        saveAccessToken(accessToken)
        if (refreshToken != null) {
            putEncrypted(KEY_REFRESH_TOKEN, refreshToken)
        }
    }

    fun saveAccessToken(token: String) {
        putEncrypted(KEY_ACCESS_TOKEN, token)
    }

    override fun getAccessToken(): String? = getDecrypted(KEY_ACCESS_TOKEN)

    fun getRefreshToken(): String? = getDecrypted(KEY_REFRESH_TOKEN)

    fun isLoggedIn(): Boolean = !getAccessToken().isNullOrBlank()

    fun clear() {
        prefs.edit().remove(KEY_ACCESS_TOKEN).remove(KEY_REFRESH_TOKEN).apply()
    }

    private fun putEncrypted(key: String, value: String) {
        val ciphertext = aead.encrypt(value.toByteArray(Charsets.UTF_8), null)
        prefs.edit().putString(key, Base64.encodeToString(ciphertext, Base64.NO_WRAP)).apply()
    }

    private fun getDecrypted(key: String): String? {
        val encoded = prefs.getString(key, null) ?: return null
        return try {
            String(aead.decrypt(Base64.decode(encoded, Base64.NO_WRAP), null), Charsets.UTF_8)
        } catch (_: GeneralSecurityException) {
            // Unreadable ciphertext (e.g. Keystore wipe) — treat as absent.
            null
        }
    }

    companion object {
        private const val PREFS_NAME = "bubutracker_session"
        private const val KEY_ACCESS_TOKEN = "access_token"
        private const val KEY_REFRESH_TOKEN = "refresh_token"
        private const val KEYSET_PREFS_NAME = "bubutracker_session_keyset"
        private const val KEYSET_NAME = "bubutracker_session_keyset_handle"
        private const val MASTER_KEY_URI = "android-keystore://bubutracker_session_master_key"

        internal fun buildAead(context: Context): Aead {
            AeadConfig.register()
            return AndroidKeysetManager.Builder()
                .withSharedPref(context, KEYSET_NAME, KEYSET_PREFS_NAME)
                .withKeyTemplate(KeyTemplates.get("AES256_GCM"))
                .withMasterKeyUri(MASTER_KEY_URI)
                .build()
                .keysetHandle
                .getPrimitive(Aead::class.java)
        }
    }
}
