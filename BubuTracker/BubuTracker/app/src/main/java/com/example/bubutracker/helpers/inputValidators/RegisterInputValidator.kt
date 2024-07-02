package com.example.bubutracker.helpers.inputValidators

import com.example.bubutracker.helpers.inputValidators.Exceptions.EmptyFieldException
import com.example.bubutracker.helpers.inputValidators.Exceptions.PasswordMismatchException

object RegisterInputValidator:InputValidator {
    @Throws(EmptyFieldException::class, PasswordMismatchException::class)
    fun validateInput(
        firstName: String?,
        lastName: String?,
        email: String?,
        password: String,
        confirmationPassword: String
    ) {
        if (isEmpty(firstName)) {
            throw EmptyFieldException("First name cannot be empty.")
        }
        if (isEmpty(lastName)) {
            throw EmptyFieldException("Last name cannot be empty.")
        }
        if (isEmpty(email)) {
            throw EmptyFieldException("Email cannot be empty.")
        }
        if (isEmpty(password)) {
            throw EmptyFieldException("Password cannot be empty.")
        }
        if (isEmpty(confirmationPassword)) {
            throw EmptyFieldException("Confirmation password cannot be empty.")
        }
        if (password != confirmationPassword) {
            throw PasswordMismatchException("Passwords do not match.")
        }
    }
    private fun isEmpty(str: String?): Boolean {
        return str == null || str.trim().isEmpty()
    }

    override fun validateInput(vararg args: String) {
        return  validateInput(
            args.getOrNull(0),
            args.getOrNull(1),
            args.getOrNull(2),
            args.getOrNull(3) ?: "",
            args.getOrNull(4) ?: ""
        );
    }
}