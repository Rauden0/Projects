package com.example.bubutracker.helpers.inputValidators

import com.example.bubutracker.helpers.inputValidators.Exceptions.EmptyFieldException
import com.example.bubutracker.helpers.inputValidators.Exceptions.PasswordMismatchException
import org.junit.Assert.assertThrows
import org.junit.Test

class RegisterInputValidatorTest {
    @Test
    fun validateInput_succeeds_withValidData() {
        RegisterInputValidator.validateInput(
            "Jane",
            "Doe",
            "jane@example.com",
            "password123",
            "password123"
        )
    }

    @Test
    fun validateInput_throws_whenFirstNameEmpty() {
        assertThrows(EmptyFieldException::class.java) {
            RegisterInputValidator.validateInput("", "Doe", "jane@example.com", "a", "a")
        }
    }

    @Test
    fun validateInput_throws_whenPasswordsMismatch() {
        assertThrows(PasswordMismatchException::class.java) {
            RegisterInputValidator.validateInput(
                "Jane",
                "Doe",
                "jane@example.com",
                "password123",
                "different"
            )
        }
    }
}
