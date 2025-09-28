package cz.muni.fi.pa165.spring2025.group1.team4.events.airplane;

import lombok.Builder;
import lombok.Getter;
import lombok.NonNull;

import java.io.Serializable;

@Builder
@Getter
public class AirplaneDto implements Serializable {
    @NonNull
    private final Long id;
}
