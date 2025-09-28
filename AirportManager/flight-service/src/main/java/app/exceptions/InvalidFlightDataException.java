package app.exceptions;

public class InvalidFlightDataException extends FlightServiceException {
    public InvalidFlightDataException() {
        super("Invalid flight data provided.");
    }
}
