package com.example.bubutracker.core.network

data class UserProfileDto(
    val id: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val markerColor: String
)

data class UserUpdateDto(
    val firstName: String?,
    val lastName: String?,
    val markerColor: String? = null
)

data class LocationUpdateData(
    val latitude: Double,
    val longitude: Double
)

data class TrackedLocationDto(
    val userId: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val markerColor: String,
    val latitude: Double?,
    val longitude: Double?,
    val updatedAt: String?
)

data class AddTrackingDto(
    val email: String
)
