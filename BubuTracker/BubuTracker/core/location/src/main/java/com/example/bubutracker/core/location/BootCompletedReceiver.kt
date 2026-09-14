package com.example.bubutracker.core.location

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import com.example.bubutracker.core.session.SessionHolder

class BootCompletedReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent?) {
        when (intent?.action) {
            Intent.ACTION_BOOT_COMPLETED,
            Intent.ACTION_MY_PACKAGE_REPLACED -> Unit
            else -> return
        }
        if (!SessionHolder.instance.isLoggedIn()) {
            return
        }
        LocationService.start(context.applicationContext)
    }
}
