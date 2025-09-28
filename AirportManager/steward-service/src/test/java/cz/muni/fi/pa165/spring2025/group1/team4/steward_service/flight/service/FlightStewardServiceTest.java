package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.service;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightStewardsService;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.StewardAssignmentEventDispatcher;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;

import java.time.LocalDateTime;
import java.util.HashSet;
import java.util.Set;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.*;

class FlightStewardServiceTest {

    @Mock
    private FlightRepository flightRepo;

    @Mock
    private StewardRepository stewardRepo;

    @Mock
    private StewardAssignmentEventDispatcher dispatcher;

    @InjectMocks
    private FlightStewardsService service;


    @BeforeEach
    void setUp() {
        MockitoAnnotations.openMocks(this);
    }

    @Test
    void findAll_ReturnsListOfStewards() throws ResourceNotFoundException {
        Steward steward = Steward.withId(2L);
        Flight flight = new Flight();
        flight.setId(1L);
        flight.setStewards(Set.of(steward));

        when(flightRepo.find(any(Flight.class))).thenReturn(flight);

        var result = service.findAll(Flight.withId(1L));

        assertThat(result).containsExactly(steward);
    }


    @Test
    void add_AddsStewardToFlight() throws Exception {
        Flight inputFlight = Flight.withId(1L);
        Flight managedFlight = new Flight();

        LocalDateTime departure = LocalDateTime.parse("2023-10-01T10:00:00");
        LocalDateTime arrival = LocalDateTime.parse("2023-10-02T10:00:00");

        managedFlight.setDepartureTime(departure);
        managedFlight.setArrivalTime(arrival);
        managedFlight.setStewards(new HashSet<>());

        Steward steward = Steward.named("Anna", "Smith");

        when(flightRepo.find(inputFlight)).thenReturn(managedFlight);
        when(flightRepo.existsFlightWithStewardBetweenGivenTimes(steward, departure, arrival)).thenReturn(false);

        service.addFlightSteward(inputFlight, steward);

        verify(flightRepo).find(inputFlight);
        verify(flightRepo).existsFlightWithStewardBetweenGivenTimes(steward, departure, arrival);
        assertThat(managedFlight.getStewards()).containsExactly(steward);
    }


    @Test
    void add_BusyDuringFlight_ThrowsConflict() throws ResourceNotFoundException {
        var steward = Steward.named("Anna", "Smith");
        steward.setId(2L);

        var flight = new Flight();
        flight.setId(1L);
        flight.setDepartureTime(LocalDateTime.parse("2023-10-01T10:00:00"));
        flight.setArrivalTime(LocalDateTime.parse("2023-10-01T11:00:00"));
        flight.setStewards(Set.of());

        when(flightRepo.find(any(Flight.class))).thenReturn(flight);
        when(flightRepo.existsFlightWithStewardBetweenGivenTimes(eq(steward), any(), any())).thenReturn(true);

        assertThatThrownBy(() ->
                service.addFlightSteward(Flight.withId(1L), Steward.withId(2L))
        ).isInstanceOf(ResourceConflictException.class);
    }


    @Test
    void remove_RemovesStewardFromFlight() throws Exception {
        Flight flight = new Flight();
        flight.setId(1L);
        Steward steward = Steward.named("Anna", "Smith");
        Set<Steward> stewards = new HashSet<>(Set.of(steward));
        flight.setStewards(stewards);

        when(flightRepo.find(flight)).thenReturn(flight);

        service.removeFlightSteward(flight, steward);

        assertThat(flight.getStewards()).doesNotContain(steward);
    }

}
