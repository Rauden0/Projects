package com.example.bubutracker.core.network

import okhttp3.Authenticator
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

    /**
     * @param enableLogging Logs full request/response bodies and headers (including the
     * bearer token). Must stay false in release builds - pass `BuildConfig.DEBUG` from the
     * app module, never hardcode true.
     * @param authenticator Recovers from a 401 by refreshing the access token and retrying
     * (see [SessionAuthenticator]). Optional so tests that don't exercise token refresh
     * (e.g. against MockWebServer with a fixed token) don't need to supply one.
     */
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
