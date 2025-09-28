package cz.muni.fi.pa165.spring2025.group1.team4.events.airport;

import lombok.Builder;
import lombok.Getter;
import lombok.NonNull;

import java.io.Serializable;

@Builder
@Getter
public class AirportDto implements Serializable {
    @NonNull
    private final Long id;
}
