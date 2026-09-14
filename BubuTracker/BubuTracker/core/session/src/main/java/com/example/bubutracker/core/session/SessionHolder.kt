package com.example.bubutracker.core.session

import android.content.Context

object SessionHolder {
    lateinit var instance: SessionManager
        private set

    fun init(context: Context) {
        instance = SessionManager(context.applicationContext)
    }
}
