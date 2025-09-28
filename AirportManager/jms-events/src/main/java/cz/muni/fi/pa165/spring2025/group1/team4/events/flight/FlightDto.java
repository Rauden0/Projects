package cz.muni.fi.pa165.spring2025.group1.team4.events.flight;

import lombok.Builder;
import lombok.Getter;
import lombok.NonNull;

import java.io.Serializable;
import java.time.LocalDateTime;

@Builder
@Getter
public class FlightDto implements Serializable {
    @NonNull
    private final Long id;

    private LocalDateTime departureTime;

    private LocalDateTime arrivalTime;
}
