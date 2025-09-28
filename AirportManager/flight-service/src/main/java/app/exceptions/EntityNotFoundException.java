package app.exceptions;

public class EntityNotFoundException extends FlightServiceException {

    public EntityNotFoundException(String entityName, Long id) {
        super(entityName + " with ID " + id + " not found.", 404);
    }

    public EntityNotFoundException(String entityName, Long id, Exception e) {
        super(entityName + " with ID " + id + " not found.", e, 404);
    }
    public EntityNotFoundException(String message)
    {
        super(message, 404);
    }
}
