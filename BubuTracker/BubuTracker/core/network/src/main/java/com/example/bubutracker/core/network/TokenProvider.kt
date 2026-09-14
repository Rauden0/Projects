package com.example.bubutracker.core.network

fun interface TokenProvider {
    fun getAccessToken(): String?
}
