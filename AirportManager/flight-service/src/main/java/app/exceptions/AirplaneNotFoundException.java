package app.exceptions;

public class AirplaneNotFoundException extends EntityNotFoundException {

    public AirplaneNotFoundException(Long id) {
        super("Airplane", id);
    }

    public AirplaneNotFoundException(Long id, Exception e) {
        super("Airplane", id, e);
    }

    public AirplaneNotFoundException(String message) {
        super(message);
    }

}
