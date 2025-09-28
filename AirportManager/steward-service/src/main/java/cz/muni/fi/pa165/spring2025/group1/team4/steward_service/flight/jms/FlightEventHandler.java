package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.Optional;
import java.util.function.Consumer;

@Component
@RequiredArgsConstructor
public class FlightEventHandler {

    @Getter(value = AccessLevel.PRIVATE)
    private final FlightService flightService;

    @Getter(value = AccessLevel.PRIVATE)
    private final JmsFlightMapper flightMapper;

    public Optional<Consumer<FlightDto>> getHandlerForType(ChangeType changeType) {
        return Optional.ofNullable(switch (changeType) {
            case CREATED -> flightCreationHandler;
            case DELETED -> flightDeletionHandler;
            case UPDATED -> flightUpdateHandler;
        });
    }

    private Consumer<FlightDto> flightCreationHandler = flight -> {
        getFlightService().create(getFlightMapper().toFlight(flight));
    };

    private Consumer<FlightDto> flightUpdateHandler = flight -> {
        getFlightService().update(getFlightMapper().toFlight(flight));
    };

    private Consumer<FlightDto> flightDeletionHandler = flight -> {
        getFlightService().deleteById(flight.getId());
    };

}
