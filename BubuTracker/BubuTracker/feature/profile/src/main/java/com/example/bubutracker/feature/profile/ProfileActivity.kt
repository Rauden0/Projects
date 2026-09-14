package com.example.bubutracker.feature.profile

import android.content.Intent
import android.content.res.ColorStateList
import android.graphics.drawable.GradientDrawable
import android.net.Uri
import android.os.Bundle
import android.view.Gravity
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.FrameLayout
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.activity.OnBackPressedCallback
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.ContextCompat
import com.example.bubutracker.core.designsystem.R as DesignSystemR
import com.example.bubutracker.core.location.LocationService
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.TrackedLocationsStore
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.network.UserProfileStore
import com.example.bubutracker.core.network.UserUpdateDto
import com.example.bubutracker.core.session.SessionHolder
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ProfileActivity : AppCompatActivity() {
    private lateinit var firstNameEditText: EditText
    private lateinit var lastNameEditText: EditText
    private lateinit var emailTextView: TextView
    private lateinit var saveButton: Button
    private lateinit var backButton: ImageButton
    private lateinit var markerColorContainer: LinearLayout

    private val swatchViews = mutableMapOf<String, FrameLayout>()
    private var selectedMarkerColor: String? = null
    private var requireName = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_edit_profile)

        requireName = intent.getBooleanExtra(AppActions.EXTRA_REQUIRE_NAME, false)

        firstNameEditText = findViewById(R.id.FirstNameTextInput)
        lastNameEditText = findViewById(R.id.LastNameTextInput)
        emailTextView = findViewById(R.id.EmailAddressTextInput)
        saveButton = findViewById(R.id.saveProfileButton)
        backButton = findViewById(R.id.backButton)
        markerColorContainer = findViewById(R.id.markerColorContainer)

        // Use OnBackPressedDispatcher — onBackPressed() is unreliable with predictive back.
        if (requireName) {
            backButton.visibility = View.GONE
            findViewById<TextView>(R.id.profileSubtitle).text = getString(R.string.edit_profile_subtitle_required)
            findViewById<TextView>(R.id.deleteAccountLink).visibility = View.GONE
            onBackPressedDispatcher.addCallback(
                this,
                object : OnBackPressedCallback(true) {
                    override fun handleOnBackPressed() {
                        showToast("Please add your name to continue.")
                    }
                }
            )
        }

        buildColorSwatches()

        val cachedProfile = UserProfileStore.getFresh()
        if (cachedProfile != null) {
            applyProfileToFields(cachedProfile)
        } else {
            ApiClient.service.getMe().enqueue(object : Callback<UserProfileDto> {
                override fun onResponse(
                    call: Call<UserProfileDto>,
                    response: Response<UserProfileDto>
                ) {
                    response.body()?.let { profile ->
                        UserProfileStore.set(profile)
                        applyProfileToFields(profile)
                    }
                }

                override fun onFailure(call: Call<UserProfileDto>, t: Throwable) = Unit
            })
        }

        saveButton.setOnClickListener { saveProfile() }
        backButton.setOnClickListener { finish() }
        findViewById<TextView>(R.id.privacyPolicyLink).setOnClickListener {
            startActivity(
                Intent(
                    Intent.ACTION_VIEW,
                    Uri.parse(getString(DesignSystemR.string.privacy_policy_url))
                )
            )
        }
        findViewById<TextView>(R.id.deleteAccountLink).setOnClickListener { confirmDeleteAccount() }
    }

    private fun applyProfileToFields(profile: UserProfileDto) {
        firstNameEditText.setText(profile.firstName)
        lastNameEditText.setText(profile.lastName)
        emailTextView.text = profile.email
        selectMarkerColor(profile.markerColor)
    }

    private fun confirmDeleteAccount() {
        AlertDialog.Builder(this)
            .setTitle(R.string.delete_account_confirm_title)
            .setMessage(R.string.delete_account_confirm_message)
            .setPositiveButton(R.string.delete_account_confirm_positive) { _, _ -> deleteAccount() }
            .setNegativeButton(R.string.delete_account_confirm_negative, null)
            .show()
    }

    private fun deleteAccount() {
        ApiClient.service.deleteMe().enqueue(object : Callback<Void> {
            override fun onResponse(call: Call<Void>, response: Response<Void>) {
                if (response.isSuccessful) {
                    LocationService.stop(this@ProfileActivity)
                    SessionHolder.instance.clear()
                    UserProfileStore.clear()
                    TrackedLocationsStore.clear()
                    val intent = Intent(AppActions.ACTION_LOGIN)
                        .setPackage(packageName)
                        .putExtra(AppActions.EXTRA_PERFORM_AUTH0_LOGOUT, true)
                        .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK)
                    startActivity(intent)
                } else {
                    showToast(getString(R.string.delete_account_failed, response.code().toString()))
                }
            }

            override fun onFailure(call: Call<Void>, t: Throwable) {
                showToast(getString(R.string.delete_account_failed, t.message ?: "network error"))
            }
        })
    }

    private fun buildColorSwatches() {
        val density = resources.displayMetrics.density
        val swatchSizePx = (SWATCH_SIZE_DP * density).toInt()
        val dotSizePx = (DOT_SIZE_DP * density).toInt()
        val marginPx = (SWATCH_MARGIN_DP * density).toInt()

        val rows = MARKER_COLORS.chunked(SWATCHES_PER_ROW)
        for (row in rows) {
            val rowLayout = LinearLayout(this).apply {
                orientation = LinearLayout.HORIZONTAL
            }
            for (colorHex in row) {
                val swatch = FrameLayout(this).apply {
                    layoutParams = LinearLayout.LayoutParams(swatchSizePx, swatchSizePx).apply {
                        marginEnd = marginPx
                        bottomMargin = marginPx
                    }
                    setOnClickListener { selectMarkerColor(colorHex) }
                }
                val dot = View(this).apply {
                    layoutParams = FrameLayout.LayoutParams(dotSizePx, dotSizePx, Gravity.CENTER)
                    background = ovalDrawable(fillColor = android.graphics.Color.parseColor(colorHex))
                }
                swatch.addView(dot)
                swatchViews[colorHex] = swatch
                rowLayout.addView(swatch)
            }
            markerColorContainer.addView(rowLayout)
        }
    }

    private fun selectMarkerColor(colorHex: String) {
        selectedMarkerColor?.let { swatchViews[it]?.background = null }

        val selected = swatchViews[colorHex]
        if (selected != null) {
            selectedMarkerColor = colorHex
            selected.background = ovalDrawable(
                fillColor = android.graphics.Color.TRANSPARENT,
                strokeColor = ContextCompat.getColor(
                    this,
                    com.example.bubutracker.core.designsystem.R.color.on_surface
                ),
                strokeWidthPx = (SELECTION_RING_WIDTH_DP * resources.displayMetrics.density)
            )
        }
    }

    private fun ovalDrawable(
        fillColor: Int,
        strokeColor: Int? = null,
        strokeWidthPx: Float = 0f
    ): GradientDrawable = GradientDrawable().apply {
        shape = GradientDrawable.OVAL
        setColor(fillColor)
        if (strokeColor != null) {
            setStroke(strokeWidthPx.toInt(), ColorStateList.valueOf(strokeColor))
        }
    }

    private fun saveProfile() {
        val firstName = firstNameEditText.text.toString().trim()
        val lastName = lastNameEditText.text.toString().trim()

        if (firstName.isEmpty() || lastName.isEmpty()) {
            showToast("First and last name are required.")
            return
        }

        ApiClient.service.updateMe(UserUpdateDto(firstName, lastName, selectedMarkerColor))
            .enqueue(object : Callback<UserProfileDto> {
                override fun onResponse(
                    call: Call<UserProfileDto>,
                    response: Response<UserProfileDto>
                ) {
                    val updated = response.body()
                    if (response.isSuccessful && updated != null) {
                        UserProfileStore.set(updated)
                        navigateTo(AppActions.ACTION_MAP, finishCurrent = true)
                    } else {
                        showToast("Profile update failed: ${response.code()}")
                    }
                }

                override fun onFailure(call: Call<UserProfileDto>, t: Throwable) {
                    showToast("Network error: ${t.message}")
                }
            })
    }

    private fun showToast(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }

    companion object {
        // Must stay in sync with chk_marker_color (db/migrations/000004) and map markers.
        private val MARKER_COLORS = listOf(
            "#FF9800", "#E91E63", "#8E24AA", "#3949AB",
            "#00897B", "#43A047", "#F4511E", "#6D4C41"
        )
        private const val SWATCHES_PER_ROW = 4
        private const val SWATCH_SIZE_DP = 48
        private const val DOT_SIZE_DP = 36
        private const val SWATCH_MARGIN_DP = 12
        private const val SELECTION_RING_WIDTH_DP = 2
    }
}
