package app.exceptions;

public class FlightNotFoundException extends EntityNotFoundException {

    public FlightNotFoundException(Long id) {
        super("Flight", id);
    }

    public FlightNotFoundException(Long id, Exception e) {
        super("Flight", id, e);
    }
    public FlightNotFoundException(String message) {
        super(message);
    }
}
