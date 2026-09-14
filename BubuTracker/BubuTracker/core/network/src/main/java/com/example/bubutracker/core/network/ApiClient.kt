package com.example.bubutracker.core.network

import okhttp3.Authenticator
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    private lateinit var apiService: ApiService

    // enableLogging logs bearer tokens — pass BuildConfig.DEBUG, never hardcode true.
    fun init(
        baseUrl: String,
        tokenProvider: TokenProvider,
        enableLogging: Boolean = false,
        authenticator: Authenticator? = null,
    ) {
        val okHttpClientBuilder = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(tokenProvider))
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
        if (enableLogging) {
            okHttpClientBuilder.addInterceptor(
                HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BODY }
            )
        }
        authenticator?.let { okHttpClientBuilder.authenticator(it) }
        val okHttpClient = okHttpClientBuilder.build()
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
