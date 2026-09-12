package com.example.bubutracker

import android.app.Application
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.session.SessionHolder

class BubuTrackerApp : Application() {
    override fun onCreate() {
        super.onCreate()
        SessionHolder.init(this)
        ApiClient.init(BuildConfig.API_BASE_URL, SessionHolder.instance)
    }
}
