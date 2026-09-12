package com.example.bubutracker.core.network

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

/**
 * Process-wide Retrofit client. [init] must be called (from the Application class in
 * production, or a test's setup) before any feature module touches [service]. Unlike a
 * `by lazy` singleton, [init] eagerly rebuilds the client every time it's called, so
 * re-initializing against a different base URL/token provider (e.g. a test's MockWebServer)
 * takes effect immediately instead of silently keeping the first configuration.
 */
object ApiClient {
    private lateinit var apiService: ApiService

    fun init(baseUrl: String, tokenProvider: TokenProvider) {
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }
        val okHttpClient = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(tokenProvider))
            .addInterceptor(loggingInterceptor)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .build()
        val retrofit = Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
        apiService = retrofit.create(ApiService::class.java)
    }

    val service: ApiService
        get() {
            check(::apiService.isInitialized) { "ApiClient.init() must be called before use" }
            return apiService
        }
}
