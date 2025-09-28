package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.facade;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.*;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.*;
@ExtendWith(MockitoExtension.class)
class FlightStewardsFacadeTest {

    @Mock
    private FlightStewardsService service;

    @Mock
    private FlightStewardsMapper mapper;

    @InjectMocks
    private FlightStewardsFacade facade;

    @Test
    void findAll_ReturnsMappedList() throws ResourceNotFoundException {
        var flightId = 10L;
        var flight = Flight.withId(flightId);
        var stewards = List.of(Steward.withId(1L), Steward.withId(2L));
        var dtos = List.of(
                new StewardDto( 1L, "Jane", "Doe"),
                new StewardDto( 2L, "John", "Smith")
        );

        when(service.findAll(flight)).thenReturn(stewards);
        when(mapper.toList(stewards)).thenReturn(dtos);

        var result = facade.findAll(flightId);

        assertThat(result).hasSize(2);
        assertThat(result.getFirst().id()).isEqualTo(1L);
        verify(service).findAll(flight);
        verify(mapper).toList(stewards);
    }

    @Test
    void add_CallsServiceWithCorrectIds() throws ResourceNotFoundException, ResourceConflictException {
        var flight = Flight.withId(5L);
        var steward = Steward.withId(99L);

        facade.addFlightSteward(flight.getId(), steward.getId());

        verify(service).addFlightSteward(flight, steward);
    }

    @Test
    void remove_CallsServiceWithCorrectIds() throws ResourceNotFoundException {
        var flight = Flight.withId(3L);
        var steward = Steward.withId(44L);

        facade.removeFlightSteward(flight.getId(), steward.getId());

        verify(service).removeFlightSteward(flight, steward);
    }
    @Test
    void findAll_WhenFlightMissing_ThrowsNotFound() throws ResourceNotFoundException {
        var flightId = 404L;
        var flight = Flight.withId(flightId);

        when(service.findAll(flight)).thenThrow(new ResourceNotFoundException("Flight not found"));

        org.junit.jupiter.api.Assertions.assertThrows(
                ResourceNotFoundException.class,
                () -> facade.findAll(flightId)
        );
    }
    @Test
    void add_WhenStewardBusy_ThrowsConflict() throws ResourceNotFoundException, ResourceConflictException {
        var flight = Flight.withId(1L);
        var steward = Steward.withId(2L);

        doThrow(new ResourceConflictException("Steward is currently busy"))
                .when(service).addFlightSteward(flight, steward);

        org.junit.jupiter.api.Assertions.assertThrows(
                ResourceConflictException.class,
                () -> facade.addFlightSteward(flight.getId(), steward.getId())
        );
    }
}
