package com.example.bubutracker.feature.auth

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.auth0.android.Auth0
import com.auth0.android.authentication.AuthenticationAPIClient
import com.auth0.android.authentication.AuthenticationException
import com.auth0.android.callback.Callback
import com.auth0.android.provider.WebAuthProvider
import com.auth0.android.result.Credentials
import com.auth0.android.result.UserProfile
import com.example.bubutracker.core.designsystem.R as DesignSystemR
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.network.UserProfileStore
import com.example.bubutracker.core.network.UserUpdateDto
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
        findViewById<TextView>(R.id.privacyPolicyLink).setOnClickListener {
            openPrivacyPolicy()
        }

        if (intent.getBooleanExtra(AppActions.EXTRA_PERFORM_AUTH0_LOGOUT, false)) {
            logoutOfAuth0Session()
        }
    }

    private fun openPrivacyPolicy() {
        startActivity(
            Intent(
                Intent.ACTION_VIEW,
                Uri.parse(getString(DesignSystemR.string.privacy_policy_url))
            )
        )
    }

    private fun logoutOfAuth0Session() {
        WebAuthProvider.logout(account)
            .withScheme(getString(R.string.com_auth0_scheme))
            .start(this, object : Callback<Void?, AuthenticationException> {
                override fun onSuccess(result: Void?) = Unit
                override fun onFailure(error: AuthenticationException) = Unit
            })
    }

    private fun loginWithAuth0() {
        WebAuthProvider.login(account)
            .withScheme(getString(R.string.com_auth0_scheme))
            .withAudience(BuildConfig.AUTH0_AUDIENCE)
            // offline_access required for Auth0 to issue a refresh token.
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
                    fetchGoogleNameThenSync(result.accessToken)
                }
            })
    }

    private fun fetchGoogleNameThenSync(accessToken: String) {
        AuthenticationAPIClient(account).userInfo(accessToken)
            .start(object : Callback<UserProfile, AuthenticationException> {
                override fun onSuccess(result: UserProfile) {
                    syncProfileAndContinue(result.givenName, result.familyName)
                }

                override fun onFailure(error: AuthenticationException) {
                    syncProfileAndContinue(null, null)
                }
            })
    }

    private fun syncProfileAndContinue(googleFirstName: String?, googleLastName: String?) {
        ApiClient.service.getMe().enqueue(object : RetrofitCallback<UserProfileDto> {
            override fun onResponse(
                call: Call<UserProfileDto>,
                response: Response<UserProfileDto>
            ) {
                if (response.isSuccessful) {
                    val profile = response.body()
                    profile?.let(UserProfileStore::set)
                    val needsName = profile != null && profile.firstName.isBlank() && profile.lastName.isBlank()
                    val hasGoogleName = !googleFirstName.isNullOrBlank() || !googleLastName.isNullOrBlank()
                    when {
                        needsName && hasGoogleName ->
                            applyGoogleName(googleFirstName.orEmpty(), googleLastName.orEmpty())
                        needsName -> {
                            navigateTo(AppActions.ACTION_PROFILE, extras = requireNameExtras())
                            finish()
                        }
                        else -> {
                            openMaps()
                            finish()
                        }
                    }
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

    private fun applyGoogleName(firstName: String, lastName: String) {
        ApiClient.service.updateMe(UserUpdateDto(firstName, lastName))
            .enqueue(object : RetrofitCallback<UserProfileDto> {
                override fun onResponse(call: Call<UserProfileDto>, response: Response<UserProfileDto>) {
                    val updated = response.body()
                    if (response.isSuccessful && updated != null) {
                        UserProfileStore.set(updated)
                        openMaps()
                    } else {
                        navigateTo(AppActions.ACTION_PROFILE, extras = requireNameExtras())
                    }
                    finish()
                }

                override fun onFailure(call: Call<UserProfileDto>, t: Throwable) {
                    navigateTo(AppActions.ACTION_PROFILE, extras = requireNameExtras())
                    finish()
                }
            })
    }

    private fun requireNameExtras() = Bundle().apply {
        putBoolean(AppActions.EXTRA_REQUIRE_NAME, true)
    }

    private fun openMaps() {
        navigateTo(AppActions.ACTION_MAP, finishCurrent = true)
    }
}
