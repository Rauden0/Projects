package app.web;

import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentEvent;
import lombok.RequiredArgsConstructor;
import org.springframework.jms.annotation.JmsListener;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class FlightChangeListener {

    private final FlightEventHandler  eventHandler;

    @JmsListener(destination = "steward-changes-for-flight-service")
    public void receiveSteward(StewardChangeEvent event) {
        eventHandler.handleStewardChange(event);
    }

    @JmsListener(destination = "steward-assignments-for-flight-service")
    public void receiveSteward(StewardAssignmentEvent event) {
        eventHandler.handleStewardAssignment(event);
    }

    @JmsListener(destination = "airplane-changes-for-flight-service")
    public void receiveAirplane(AirplaneChangeEvent event) {
        eventHandler.handleAirplaneChange(event);
    }

    @JmsListener(destination = "airport-changes-for-flight-service")
    public void receiveAirport(AirportChangeEvent event) {
        eventHandler.handleAirportChange(event);
    }
}

