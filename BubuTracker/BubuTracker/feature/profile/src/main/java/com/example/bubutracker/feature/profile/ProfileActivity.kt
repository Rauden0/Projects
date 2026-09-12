package com.example.bubutracker.feature.profile

import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.example.bubutracker.core.navigation.AppActions
import com.example.bubutracker.core.navigation.navigateTo
import com.example.bubutracker.core.network.ApiClient
import com.example.bubutracker.core.network.UserProfileDto
import com.example.bubutracker.core.network.UserUpdateDto
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ProfileActivity : AppCompatActivity() {
    private lateinit var firstNameEditText: EditText
    private lateinit var lastNameEditText: EditText
    private lateinit var saveButton: Button

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_edit_profile)

        firstNameEditText = findViewById(R.id.FirstNameTextInput)
        lastNameEditText = findViewById(R.id.LastNameTextInput)
        saveButton = findViewById(R.id.registerConfirmationButton)
        findViewById<EditText>(R.id.EmailAddressTextInput).isEnabled = false
        findViewById<EditText>(R.id.PasswordTextInput).visibility = View.GONE
        findViewById<EditText>(R.id.PasswordConfirmationTextInput).visibility = View.GONE
        saveButton.text = getString(R.string.save_profile)

        ApiClient.service.getMe().enqueue(object : Callback<UserProfileDto> {
            override fun onResponse(
                call: Call<UserProfileDto>,
                response: Response<UserProfileDto>
            ) {
                response.body()?.let { profile ->
                    firstNameEditText.setText(profile.firstName)
                    lastNameEditText.setText(profile.lastName)
                    findViewById<EditText>(R.id.EmailAddressTextInput).setText(profile.email)
                }
            }

            override fun onFailure(call: Call<UserProfileDto>, t: Throwable) = Unit
        })

        saveButton.setOnClickListener { saveProfile() }
    }

    private fun saveProfile() {
        val firstName = firstNameEditText.text.toString().trim()
        val lastName = lastNameEditText.text.toString().trim()

        if (firstName.isEmpty() || lastName.isEmpty()) {
            showToast("First and last name are required.")
            return
        }

        ApiClient.service.updateMe(UserUpdateDto(firstName, lastName))
            .enqueue(object : Callback<UserProfileDto> {
                override fun onResponse(
                    call: Call<UserProfileDto>,
                    response: Response<UserProfileDto>
                ) {
                    if (response.isSuccessful) {
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
}
