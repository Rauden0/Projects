package com.example.bubutracker.core.network

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

    /** Other users' pending requests to track me - the consent inbox. */
    @GET("api/v1/tracking/requests")
    fun getIncomingTrackingRequests(): Call<List<UserProfileDto>>

    /** Grants trackerId visibility into my location. */
    @POST("api/v1/tracking/requests/{trackerId}/accept")
    fun acceptTrackingRequest(@Path("trackerId") trackerId: String): Call<Void>

    /** Users currently, with my consent, tracking me. */
    @GET("api/v1/tracking/followers")
    fun getFollowers(): Call<List<UserProfileDto>>

    /** Rejects a pending request from trackerId, or revokes their already-accepted access. */
    @DELETE("api/v1/tracking/followers/{trackerId}")
    fun removeFollower(@Path("trackerId") trackerId: String): Call<Void>
}
