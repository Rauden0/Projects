package com.example.bubutracker.client

import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.Path

interface ApiService {
    @GET("api/v1/users/me")
    fun getMe(): Call<UserProfileDto>

    @PATCH("api/v1/users/me")
    fun updateMe(@Body update: UserUpdateDto): Call<UserProfileDto>

    @POST("api/v1/locations/me")
    fun updateLocation(@Body location: LocationUpdateData): Call<Void>

    @GET("api/v1/locations/tracked")
    fun getTrackedLocations(): Call<List<TrackedLocationDto>>

    @GET("api/v1/tracking")
    fun getTrackedUsers(): Call<List<UserProfileDto>>

    @POST("api/v1/tracking")
    fun addTracking(@Body request: AddTrackingDto): Call<Void>

    @DELETE("api/v1/tracking/{userId}")
    fun removeTracking(@Path("userId") userId: String): Call<Void>
}
