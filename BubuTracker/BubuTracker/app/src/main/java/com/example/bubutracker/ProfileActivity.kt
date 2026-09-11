package com.example.bubutracker

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.example.bubutracker.client.RetrofitClient
import com.example.bubutracker.client.UserUpdateDto
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ProfileActivity : AppCompatActivity() {
    private lateinit var firstNameEditText: EditText
    private lateinit var lastNameEditText: EditText
    private lateinit var saveButton: Button

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_register)

        firstNameEditText = findViewById(R.id.FirstNameTextInput)
        lastNameEditText = findViewById(R.id.LastNameTextInput)
        saveButton = findViewById(R.id.registerConfirmationButton)
        findViewById<EditText>(R.id.EmailAddressTextInput).isEnabled = false
        findViewById<EditText>(R.id.PasswordTextInput).visibility = android.view.View.GONE
        findViewById<EditText>(R.id.PasswordConfirmationTextInput).visibility = android.view.View.GONE
        saveButton.text = getString(R.string.save_profile)

        RetrofitClient.apiService.getMe().enqueue(object : Callback<com.example.bubutracker.client.UserProfileDto> {
            override fun onResponse(
                call: Call<com.example.bubutracker.client.UserProfileDto>,
                response: Response<com.example.bubutracker.client.UserProfileDto>
            ) {
                response.body()?.let { profile ->
                    firstNameEditText.setText(profile.firstName)
                    lastNameEditText.setText(profile.lastName)
                    findViewById<EditText>(R.id.EmailAddressTextInput).setText(profile.email)
                }
            }

            override fun onFailure(
                call: Call<com.example.bubutracker.client.UserProfileDto>,
                t: Throwable
            ) = Unit
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

        RetrofitClient.apiService.updateMe(UserUpdateDto(firstName, lastName))
            .enqueue(object : Callback<com.example.bubutracker.client.UserProfileDto> {
                override fun onResponse(
                    call: Call<com.example.bubutracker.client.UserProfileDto>,
                    response: Response<com.example.bubutracker.client.UserProfileDto>
                ) {
                    if (response.isSuccessful) {
                        startActivity(Intent(this@ProfileActivity, MapsActivity::class.java))
                        finish()
                    } else {
                        showToast("Profile update failed: ${response.code()}")
                    }
                }

                override fun onFailure(
                    call: Call<com.example.bubutracker.client.UserProfileDto>,
                    t: Throwable
                ) {
                    showToast("Network error: ${t.message}")
                }
            })
    }

    private fun showToast(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }
}
