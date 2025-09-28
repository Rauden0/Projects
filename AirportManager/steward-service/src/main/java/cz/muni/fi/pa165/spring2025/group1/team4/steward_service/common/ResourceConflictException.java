package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import lombok.experimental.StandardException;

@StandardException
public class ResourceConflictException extends Exception {

    public static ResourceConflictException notAvailable(String resourceType) {
        return new ResourceConflictException(String.format("Provided %s is not available", resourceType));
    }

}
