package com.example.bubutracker

import android.app.Application
import com.example.bubutracker.sessionManager.SessionManager

class BubuTrackerApp : Application() {
    override fun onCreate() {
        super.onCreate()
        sessionManager = SessionManager(this)
    }

    companion object {
        lateinit var sessionManager: SessionManager
            private set
    }
}
