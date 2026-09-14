package com.example.bubutracker.core.navigation

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.os.Bundle

fun Context.navigateTo(action: String, finishCurrent: Boolean = false, extras: Bundle? = null) {
    val intent = Intent(action).setPackage(packageName)
    if (extras != null) {
        intent.putExtras(extras)
    }
    startActivity(intent)
    if (finishCurrent && this is Activity) {
        finish()
    }
}
