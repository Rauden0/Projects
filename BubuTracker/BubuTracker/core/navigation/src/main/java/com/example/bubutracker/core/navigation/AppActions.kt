package com.example.bubutracker.core.navigation

object AppActions {
    const val ACTION_LOGIN = "com.example.bubutracker.action.LOGIN"
    const val ACTION_MAP = "com.example.bubutracker.action.MAP"
    const val ACTION_PROFILE = "com.example.bubutracker.action.PROFILE"

    // Clear Auth0 browser SSO before showing login again.
    const val EXTRA_PERFORM_AUTH0_LOGOUT = "com.example.bubutracker.extra.PERFORM_AUTH0_LOGOUT"

    // Profile must collect a name before the user can leave the screen.
    const val EXTRA_REQUIRE_NAME = "com.example.bubutracker.extra.REQUIRE_NAME"
}
