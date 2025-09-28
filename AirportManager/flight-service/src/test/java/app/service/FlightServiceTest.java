package app.service;

import app.common.TestDataGenerator;
import app.entity.Flight;
import app.mapper.FlightEventMapper;
import app.repository.FlightRepository;
import app.web.FlightChangeProducer;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.PageRequest;

import java.util.List;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class FlightServiceTest {
    @Mock
    private FlightRepository flightRepository;

    @InjectMocks
    private FlightService flightService;

    @Mock
    private FlightEventMapper flightEventMapper;

    @Mock
    private FlightChangeProducer flightChangeProducer;

    @Test
    void getFlightById_ReturnsFlightFromRepo_IfFlightExists() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);

        flight.setId(310L); // No trip sanity check

        when(flightRepository.findById(310L)).thenReturn(Optional.of(flight));

        var flightFromService = flightService.getFlightById(310L);

        assertThat(flightFromService).contains(flight);
    }

    @Test
    void getFlightById_ReturnsEmpty_IfFlightDoesNotExist() {
        when(flightRepository.findById(310L)).thenReturn(Optional.empty());

        var flightFromService = flightService.getFlightById(310L);

        verify(flightRepository).findById(310L);

        assertThat(flightFromService).isEmpty();
    }

    @Test
    void createFlight_ReturnsFlightFromRepo() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var flightWithId = TestDataGenerator.getDefaultFor(Flight.class);

        flightWithId.setId(310L); // No trip sanity check

        when(flightRepository.save(flight)).thenReturn(flightWithId);

        var flightFromService = flightService.createFlight(flight);

        verify(flightRepository).save(flight);

        assertThat(flightFromService).isEqualTo(flightWithId);
    }

    @Test
    void deleteFlight_CallsRepoMethod() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        flight.setId(310L);
        when(flightRepository.findById(310L)).thenReturn(Optional.of(flight));
        doNothing().when(flightChangeProducer).sendFlightChange(any(FlightChangeEvent.class));
        flightService.deleteFlight(310L);
        verify(flightRepository).deleteById(310L);
    }


    @Test
    void updateFlight_ReturnsFlightFromRepoMethod() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var flightFromRepo = TestDataGenerator.getDefaultFor(Flight.class);

        flight.setId(310L);
        flightFromRepo.setId(310L);
        lenient().when(flightRepository.findById(310L)).thenReturn(Optional.of(flightFromRepo));

        when(flightRepository.save(flight)).thenReturn(flightFromRepo);

        var flightFromService = flightService.updateFlight(flight);

        verify(flightRepository).save(flight);

        assertThat(flightFromService).isEqualTo(flightFromRepo);
    }

    @Test
    void getFlightByCode_ReturnsFlightFromRepoMethod() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var code = flight.getFlightCode();

        // No trip sanity checks
        flight.setId(310L);

        when(flightRepository.findByFlightCode(code)).thenReturn(Optional.of(flight));

        var flightFromService = flightService.getFlightByCode(code);

        verify(flightRepository).findByFlightCode(code);

        assertThat(flightFromService).contains(flight);
    }

    @Test
    void getFlightCode_ReturnsEmpty_WhenFlightNotInRepo() {
        var code = TestDataGenerator.getDefaultFor(Flight.class).getFlightCode();

        when(flightRepository.findByFlightCode(code)).thenReturn(Optional.empty());

        var flightFromService = flightService.getFlightByCode(code);

        verify(flightRepository).findByFlightCode(code);

        assertThat(flightFromService).isEmpty();
    }

    @Test
    void getAllFlights_ReturnsFlightsFromRepo() {
        var flights = TestDataGenerator.first(4).instancesOf(Flight.class);
        Page<Flight> flightPage = new PageImpl<>(flights, PageRequest.of(1, 10), flights.size());
        when(flightRepository.findAll(PageRequest.of(1,10))).thenReturn(flightPage);

        var flightsFromService = flightService.getAllFlights(PageRequest.of(1,10));

        verify(flightRepository).findAll(PageRequest.of(1,10));

        assertThat(flightsFromService).containsExactlyInAnyOrderElementsOf(flights);
    }

    @Test
    void getAllFlights_IsEmpty_IfNoFlightsInRepo() {
        Page<Flight> flightPage = new PageImpl<>(List.of(), PageRequest.of(1, 10),0);

        when(flightRepository.findAll(PageRequest.of(1,10))).thenReturn(flightPage);

        var flightsFromService = flightService.getAllFlights(PageRequest.of(1,10));

        verify(flightRepository).findAll(PageRequest.of(1,10));

        assertThat(flightsFromService).isEmpty();
    }
}
