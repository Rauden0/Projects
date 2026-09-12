package com.example.bubutracker.core.navigation

import android.app.Activity
import android.content.Context
import android.content.Intent

/**
 * Starts the Activity registered for [action] in the current app package and,
 * when called from an Activity, optionally finishes it.
 */
fun Context.navigateTo(action: String, finishCurrent: Boolean = false) {
    startActivity(Intent(action).setPackage(packageName))
    if (finishCurrent && this is Activity) {
        finish()
    }
}
