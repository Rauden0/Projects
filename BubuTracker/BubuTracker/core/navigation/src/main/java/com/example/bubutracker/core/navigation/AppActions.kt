package com.example.bubutracker.core.navigation

/**
 * Custom intent actions used to move between feature screens without any feature
 * module compile-depending on another. Each destination Activity declares an
 * intent-filter for its action in its own module's AndroidManifest.xml.
 */
object AppActions {
    const val ACTION_LOGIN = "com.example.bubutracker.action.LOGIN"
    const val ACTION_MAP = "com.example.bubutracker.action.MAP"
    const val ACTION_PROFILE = "com.example.bubutracker.action.PROFILE"
}
