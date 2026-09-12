package com.example.bubutracker.feature.auth

import android.os.Bundle
import android.widget.Button
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.auth0.android.Auth0
import com.auth0.android.authentication.AuthenticationException
import com.auth0.android.callback.Callback
import com.auth0.android.provider.WebAuthProvider
import com.auth0.android.result.Credentials
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.session.SessionHolder
import retrofit2.Call
import retrofit2.Callback as RetrofitCallback
import retrofit2.Response

class LoginActivity : AppCompatActivity() {
    private lateinit var account: Auth0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        if (SessionHolder.instance.isLoggedIn()) {
            openMaps()
            return
        }

        setContentView(R.layout.activity_login)
        account = Auth0.getInstance(
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
            // offline_access is required for Auth0 to issue a refresh token; without it
            // SessionAuthenticator has nothing to refresh with once the access token
            // expires, and every request just fails with 401 from then on.
            .withScope("openid profile email offline_access")
            .start(this, object : Callback<Credentials, AuthenticationException> {
                override fun onFailure(error: AuthenticationException) {
                    Toast.makeText(
                        this@LoginActivity,
                        "Login failed: ${error.getDescription()}",
                        Toast.LENGTH_LONG
                    ).show()
                }

                override fun onSuccess(result: Credentials) {
                    SessionHolder.instance.saveCredentials(result.accessToken, result.refreshToken)
                    syncProfileAndContinue()
                }
            })
    }

    private fun syncProfileAndContinue() {
        ApiClient.service.getMe().enqueue(object : RetrofitCallback<UserProfileDto> {
            override fun onResponse(
                call: Call<UserProfileDto>,
                response: Response<UserProfileDto>
            ) {
                if (response.isSuccessful) {
                    val profile = response.body()
                    if (profile != null && profile.firstName.isBlank() && profile.lastName.isBlank()) {
                        navigateTo(AppActions.ACTION_PROFILE)
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

            override fun onFailure(call: Call<UserProfileDto>, t: Throwable) {
                Toast.makeText(
                    this@LoginActivity,
                    "Network error: ${t.message}",
                    Toast.LENGTH_LONG
                ).show()
            }
        })
    }

    private fun openMaps() {
        navigateTo(AppActions.ACTION_MAP, finishCurrent = true)
    }
}
