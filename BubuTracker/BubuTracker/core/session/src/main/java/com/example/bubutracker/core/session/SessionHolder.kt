package com.example.bubutracker.core.session

import android.content.Context

/**
 * Process-wide access point to [SessionManager]. [init] must be called once
 * (from the Application class) before any feature module reads [instance].
 */
object SessionHolder {
    lateinit var instance: SessionManager
        private set

    fun init(context: Context) {
        instance = SessionManager(context.applicationContext)
    }
}
