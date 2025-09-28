package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import lombok.RequiredArgsConstructor;
import org.springframework.jms.annotation.JmsListener;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class FlightChangeListener {

    private final FlightEventHandler flightEventHandler;

    @JmsListener(destination = "flight-changes-for-steward-service")
    public void accept(FlightChangeEvent event) {
        flightEventHandler.getHandlerForType(event.changeType()).ifPresent(handler -> handler.accept(event.flight()));
    }
}
