package app.web;

import app.entity.Airplane;
import app.entity.Airport;
import app.entity.Flight;
import app.service.AirplaneService;
import app.service.AirportService;
import app.service.FlightService;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentEvent;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Component
@RequiredArgsConstructor
public class FlightEventHandler {

    private final FlightService flightService;
    private final AirplaneService airplaneService;
    private final AirportService airportService;

    @Transactional
    public void handleAirportChange(AirportChangeEvent event) {
        switch (event.changeType()) {
            case DELETED -> {
                flightService.deleteFlightsByAirport(event.airport().getId());
                airportService.deleteAirport(event.airport().getId());
            }
            case CREATED -> airportService.createAirport(new Airport(event.airport().getId()));
            case UPDATED -> {
                // No action: ID should not be updated
            }
        }
    }

    @Transactional
    public void handleAirplaneChange(AirplaneChangeEvent event) {
        switch (event.changeType()) {
            case DELETED -> {
                flightService.deleteFlightsByAirplaneId(event.airplane().getId());
                airplaneService.deleteAirplane(event.airplane().getId());
            }
            case CREATED -> airplaneService.createAirplane(new Airplane(event.airplane().getId()));
            case UPDATED -> {
                // No action: ID should not be updated
            }
        }
    }

    @Transactional
    public void handleStewardChange(StewardChangeEvent event) {
        switch (event.changeType()) {
            case DELETED -> {
                List<Flight> flights = flightService.getFlightsWithSteward(event.steward().getId());
                for (Flight flight : flights) {
                    flightService.unassignSteward(flight.getId(), event.steward().getId());
                }
            }

            case CREATED,
                 UPDATED -> {
                // No action: ID should not be updated
            }

        }
    }

    @Transactional
    public void handleStewardAssignment(StewardAssignmentEvent event) {
        switch (event.changeType()) {
            case DELETED ->
                    flightService.unassignSteward(event.steward().getFlightId(), event.steward().getStewardId());
            case CREATED -> flightService.addSteward(event.steward().getFlightId(), event.steward().getStewardId());
        }
    }
}
