package app.integration;

import app.common.TestDataGenerator;
import app.entity.Airplane;
import app.entity.Airport;
import app.entity.Flight;
import app.repository.AirplaneRepository;
import app.repository.AirportRepository;
import app.repository.FlightRepository;
import app.service.FlightService;
import app.web.FlightEventHandler;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneDto;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportDto;
import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardDto;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentDto;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentEvent;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.test.context.ActiveProfiles;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;

@ActiveProfiles("test")
@DirtiesContext(classMode = DirtiesContext.ClassMode.AFTER_EACH_TEST_METHOD)
@SpringBootTest
public class FlightEventHandlerIntegrationTest {

    @Autowired
    private FlightEventHandler flightEventHandler;

    @Autowired
    private AirportRepository airportRepository;

    @Autowired
    private AirplaneRepository airplaneRepository;

    @Autowired
    private FlightRepository flightRepository;

    @Autowired
    private FlightService flightService;

    @BeforeEach
    void setup() {
        flightRepository.deleteAll();
        airportRepository.deleteAll();
        airplaneRepository.deleteAll();
    }

    @Test
    void handleAirportCreated_savesAirport() {
        var event = new AirportChangeEvent(ChangeType.CREATED, toDto(new Airport(100L)));
        flightEventHandler.handleAirportChange(event);
        assertThat(airportRepository.findById(100L)).isPresent();
    }

    @Test
    void handleAirportDeleted_deletesAirportAndFlights() {
        Airport airport = airportRepository.save(new Airport(200L));
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setArrivalAirportId(200L);
        flight.setDepartureAirportId(200L);
        flightRepository.save(flight);

        flightEventHandler.handleAirportChange(new AirportChangeEvent(ChangeType.DELETED, toDto(airport)));

        assertThat(airportRepository.findById(200L)).isEmpty();
        assertThat(flightRepository.findAll()).isEmpty();
    }

    @Test
    void handleAirportUpdated_doesNothing() {
        var airport = airportRepository.save(new Airport(123L));
        flightEventHandler.handleAirportChange(new AirportChangeEvent(ChangeType.UPDATED, toDto(airport)));

        assertThat(airportRepository.findById(123L)).isPresent();
    }

    @Test
    void handleAirplaneCreated_savesAirplane() {
        flightEventHandler.handleAirplaneChange(
                new AirplaneChangeEvent(ChangeType.CREATED, toDto(new Airplane(300L)))
        );
        assertThat(airplaneRepository.findById(300L)).isPresent();
    }

    @Test
    void handleAirplaneDeleted_deletesAirplaneAndFlights() {
        airplaneRepository.save(new Airplane(301L));
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setPlaneId(301L);
        flightRepository.save(flight);

        flightEventHandler.handleAirplaneChange(
                new AirplaneChangeEvent(ChangeType.DELETED, toDto(new Airplane(301L)))
        );

        assertThat(airplaneRepository.findById(301L)).isEmpty();
        assertThat(flightRepository.findAll()).isEmpty();
    }

    @Test
    void handleAirplaneUpdated_doesNothing() {
        Airplane airplane = airplaneRepository.save(new Airplane(302L));
        flightEventHandler.handleAirplaneChange(
                new AirplaneChangeEvent(ChangeType.UPDATED, toDto(airplane))
        );

        assertThat(airplaneRepository.findById(302L)).isPresent();
    }

    @Test
    void handleStewardDeleted_removesStewardFromAllFlights() {
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setFlightCode("STEWARD-OUT");
        flight.setStewardsIds(List.of(500L));
        flightRepository.save(flight);

        flightEventHandler.handleStewardChange(
                new StewardChangeEvent(ChangeType.DELETED, StewardDto.builder().id(500L).build())
        );

        Flight updated = flightRepository.findByFlightCodeWithStewards("STEWARD-OUT").orElseThrow();
        assertThat(updated.getStewardsIds()).doesNotContain(500L);
    }

    @Test
    void handleStewardUpdated_addsStewardToFlightAgain() {
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setFlightCode("FLIGHT-UPDATE");
        flightRepository.save(flight);

        flightEventHandler.handleStewardAssignment(
                new StewardAssignmentEvent(ChangeType.CREATED, StewardAssignmentDto.builder().stewardId(600L).flightId( flight.getId()).build())
        );

        Flight updated = flightRepository.findByFlightCodeWithStewards("FLIGHT-UPDATE").orElseThrow();
        assertThat(updated.getStewardsIds()).contains(600L);
    }

    @Test
    void handleStewardUnnasignment()
    {
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setFlightCode("FLIGHT-UNASSIGN");
        flight.setStewardsIds(List.of(700L));
        flightRepository.save(flight);
        flightEventHandler.handleStewardAssignment(
                new StewardAssignmentEvent(ChangeType.DELETED, StewardAssignmentDto.builder().stewardId(700L).flightId(flight.getId()).build())
        );

        Flight updated = flightRepository.findByFlightCodeWithStewards("FLIGHT-UNASSIGN").orElseThrow();
        assertThat(updated.getStewardsIds()).doesNotContain(700L);
    }


    @Test
    void deletingStewardOnUnrelatedFlightsDoesNothing() {
        Flight flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setFlightCode("A");
        flight.setStewardsIds(List.of(1L, 2L, 3L));
        flightRepository.save(flight);

        flightEventHandler.handleStewardChange(
                new StewardChangeEvent(ChangeType.DELETED,  StewardDto.builder().id(999L).build()));

        Flight updated = flightRepository.findByFlightCodeWithStewards("A").orElseThrow();
        assertThat(updated.getStewardsIds()).containsExactly(1L, 2L, 3L);
    }

    // === DTO MAPPERS ===

    private AirportDto toDto(Airport airport) {
        return AirportDto.builder().id(airport.getId()).build();
    }

    private AirplaneDto toDto(Airplane airplane) {
        return AirplaneDto.builder().id(airplane.getId()).build();
    }

}
