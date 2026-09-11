package com.example.bubutracker

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.auth0.android.Auth0
import com.auth0.android.authentication.AuthenticationException
import com.auth0.android.callback.Callback
import com.auth0.android.provider.WebAuthProvider
import com.auth0.android.result.Credentials
import com.example.bubutracker.client.RetrofitClient
import retrofit2.Call
import retrofit2.Callback as RetrofitCallback
import retrofit2.Response

class LoginActivity : AppCompatActivity() {
    private lateinit var account: Auth0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        if (BubuTrackerApp.sessionManager.isLoggedIn()) {
            openMaps()
            return
        }

        setContentView(R.layout.activity_login)
        account = Auth0(
            BuildConfig.AUTH0_CLIENT_ID,
            BuildConfig.AUTH0_DOMAIN
        )

        findViewById<Button>(R.id.loginButton).setOnClickListener {
            loginWithAuth0()
        }
    }

    private fun loginWithAuth0() {
        WebAuthProvider.login(account)
            .withScheme(getString(R.string.com_auth0_scheme))
            .withAudience(BuildConfig.AUTH0_AUDIENCE)
            .withScope("openid profile email")
            .start(this, object : Callback<Credentials, AuthenticationException> {
                override fun onFailure(error: AuthenticationException) {
                    Toast.makeText(
                        this@LoginActivity,
                        "Login failed: ${error.getDescription()}",
                        Toast.LENGTH_LONG
                    ).show()
                }

                override fun onSuccess(result: Credentials) {
                    BubuTrackerApp.sessionManager.saveAccessToken(result.accessToken)
                    syncProfileAndContinue()
                }
            })
    }

    private fun syncProfileAndContinue() {
        RetrofitClient.apiService.getMe().enqueue(object : RetrofitCallback<com.example.bubutracker.client.UserProfileDto> {
            override fun onResponse(
                call: Call<com.example.bubutracker.client.UserProfileDto>,
                response: Response<com.example.bubutracker.client.UserProfileDto>
            ) {
                if (response.isSuccessful) {
                    val profile = response.body()
                    if (profile != null && profile.firstName.isBlank() && profile.lastName.isBlank()) {
                        startActivity(Intent(this@LoginActivity, ProfileActivity::class.java))
                    } else {
                        openMaps()
                    }
                    finish()
                    return
                }

                Toast.makeText(
                    this@LoginActivity,
                    "API sync failed: ${response.code()}",
                    Toast.LENGTH_LONG
                ).show()
            }

            override fun onFailure(
                call: Call<com.example.bubutracker.client.UserProfileDto>,
                t: Throwable
            ) {
                Toast.makeText(
                    this@LoginActivity,
                    "Network error: ${t.message}",
                    Toast.LENGTH_LONG
                ).show()
            }
        })
    }

    private fun openMaps() {
        startActivity(Intent(this, MapsActivity::class.java))
    }
}
