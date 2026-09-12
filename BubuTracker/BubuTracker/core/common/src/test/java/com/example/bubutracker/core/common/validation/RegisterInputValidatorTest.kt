package com.example.bubutracker.core.common.validation

import com.example.bubutracker.core.common.validation.exceptions.EmptyFieldException
import com.example.bubutracker.core.common.validation.exceptions.PasswordMismatchException
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
