package app.exceptions;

import lombok.Getter;

@Getter
public class FlightServiceException extends RuntimeException {
    private final int returnCode;

    public FlightServiceException(String message) {
        this(message, 500);
    }

    public FlightServiceException(String message, int returnCode) {
        super(message);
        this.returnCode = returnCode;
    }

    public FlightServiceException(String message, Throwable cause) {
        this(message, cause, 500);
    }

    public FlightServiceException(String message, Throwable cause, int returnCode) {
        super(message, cause);
        this.returnCode = returnCode;
    }

    public FlightServiceException(Throwable cause) {
        this(cause, 500);
    }

    public FlightServiceException(Throwable cause, int returnCode) {
        super(cause);
        this.returnCode = returnCode;
    }

}
