package app.exceptions;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.logging.Logger;

@RestControllerAdvice
public class GlobalExceptionHandler {

    private static final Logger logger = Logger.getLogger(GlobalExceptionHandler.class.getName());

    @ExceptionHandler(FlightServiceException.class)
    public ResponseEntity<String> handleFlightNotFound(FlightServiceException ex) {
        logger.severe("Error: " + ex.getMessage());
        return new ResponseEntity<>(ex.getMessage(), HttpStatus.valueOf(ex.getReturnCode()));
    }
}
