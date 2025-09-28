package app.exceptions;

public class AirportNotFoundException extends EntityNotFoundException {

    public AirportNotFoundException(Long id) {
        super("Airport", id);
    }

    public AirportNotFoundException(Long id, Exception e) {
        super("Airport", id, e);
    }

    public AirportNotFoundException(String message) {
        super(message);
    }
}
