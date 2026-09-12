package com.example.bubutracker.core.network

/**
 * Supplies the bearer token for outgoing API calls without coupling the network
 * layer to a concrete session/auth implementation (implemented by :core:session).
 */
fun interface TokenProvider {
    fun getAccessToken(): String?
}
