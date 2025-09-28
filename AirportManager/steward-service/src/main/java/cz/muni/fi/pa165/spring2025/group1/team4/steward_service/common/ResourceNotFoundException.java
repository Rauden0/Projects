package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import lombok.experimental.StandardException;

@StandardException
public class ResourceNotFoundException extends Exception {

    public static ResourceNotFoundException notFound(String resourceType) {
        return new ResourceNotFoundException(
                String.format("Could not find provided %s", resourceType));
    }

}
