package com.example.bubutracker
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity

import com.example.bubutracker.client.RegisterData
import com.example.bubutracker.client.RetrofitClient
import com.example.bubutracker.helpers.inputValidators.Exceptions.EmptyFieldException
import com.example.bubutracker.helpers.inputValidators.Exceptions.PasswordMismatchException
import com.example.bubutracker.helpers.inputValidators.RegisterInputValidator
import retrofit2.Call
import retrofit2.Response
import retrofit2.*

class RegisterActivity : AppCompatActivity() {
    private lateinit var firstNameEditText: EditText
    private lateinit var lastNameEditText: EditText
    private lateinit var emailEditText: EditText
    private lateinit var passwordEditText: EditText
    private lateinit var passwordConfirmationEditText: EditText
    private lateinit var registerButton: Button

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_register)

        registerButton = findViewById(R.id.registerConfirmationButton)
        registerButton.post {
            firstNameEditText = findViewById(R.id.FirstNameTextInput)
            lastNameEditText = findViewById(R.id.LastNameTextInput)
            emailEditText = findViewById(R.id.EmailAddressTextInput)
            passwordEditText = findViewById(R.id.PasswordTextInput)
            passwordConfirmationEditText = findViewById(R.id.PasswordConfirmationTextInput)
            registerButton.setOnClickListener {
                validateAndRegister()
            }
        }
    }

    override fun onResume() {
        super.onResume()
        firstNameEditText = findViewById(R.id.FirstNameTextInput)
        lastNameEditText = findViewById(R.id.LastNameTextInput)
        emailEditText = findViewById(R.id.EmailAddressTextInput)
        passwordEditText = findViewById(R.id.PasswordTextInput)
        passwordConfirmationEditText = findViewById(R.id.PasswordConfirmationTextInput)
        registerButton = findViewById(R.id.registerConfirmationButton)
        registerButton.setOnClickListener {
            validateAndRegister()
        }
    }
    private fun validateAndRegister() {
        showToast("Started")
        val firstName = firstNameEditText.text.toString()
        val lastName = lastNameEditText.text.toString()
        val email = emailEditText.text.toString()
        val password = passwordEditText.text.toString()
        val confirmationPassword = passwordConfirmationEditText.text.toString()

        try {
            RegisterInputValidator.validateInput(firstName, lastName, email, password, confirmationPassword)
            performRegistration(firstName, lastName, email, password)
        } catch (e: EmptyFieldException) {
            showToast(e.message.toString())
        } catch (e: PasswordMismatchException) {
            showToast(e.message.toString())
        }
    }

    private fun performRegistration(firstName: String, lastName: String, email: String, password: String) {
        val data = RegisterData(firstName, lastName, email, password)
        val call = RetrofitClient.apiService.registerUser(data)

        call.enqueue(object : Callback<Void> {
            override fun onResponse(call: Call<Void>, response: Response<Void>) {
                if (response.isSuccessful) {
                    Toast.makeText(this@RegisterActivity, "Registration successful!", Toast.LENGTH_SHORT).show()
                } else {
                    Toast.makeText(this@RegisterActivity, "Registration failed: ${response.code()}", Toast.LENGTH_SHORT).show()
                }
            }

            override fun onFailure(call: Call<Void>, t: Throwable) {
                Toast.makeText(this@RegisterActivity, "Network error: ${t.message}", Toast.LENGTH_SHORT).show()
            }
        })
    }
    private fun showToast(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }


}
