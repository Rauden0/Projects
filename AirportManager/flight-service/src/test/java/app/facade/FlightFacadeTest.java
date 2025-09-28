package app.facade;

import app.common.TestDataGenerator;
import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.entity.Airplane;
import app.entity.Airport;
import app.entity.Flight;
import app.mapper.FlightMapper;
import app.service.AirplaneService;
import app.service.AirportService;
import app.service.FlightService;
import app.struct.Status;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Captor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.domain.Pageable;

import java.io.IOException;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.function.BiConsumer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class FlightFacadeTest {

    @Mock private FlightService flightService;
    @Mock private FlightMapper flightMapper;
    @Mock private AirportService airportService;
    @Mock private AirplaneService airplaneService;

    @InjectMocks
    private FlightFacade flightFacade;

    @Captor
    ArgumentCaptor<List<Flight>> flightsForMapper;

    private static final BiConsumer<? super Flight, Flight> weakEquality = (flight1, flight2) -> {
        assertThat(flight1.getFlightCode()).isEqualTo(flight2.getFlightCode());
    };

    @Test
    void getAllFlights_ShouldUseFlightsFromService() {
        var generator = TestDataGenerator.first(6);

        var flights = generator.instancesOf(Flight.class);
        var flightsUntouched = generator.instancesOf(Flight.class);
        var desiredFlightDtos = generator.instancesOf(FlightDTO.class);

        when(flightService.getAllFlights(Pageable.unpaged())).thenReturn(flights);
        when(flightMapper.toDTOList(flights)).thenReturn(desiredFlightDtos);

        var flightsFromFacade = flightFacade.getAllFlights(Pageable.unpaged());

        verify(flightService).getAllFlights(Pageable.unpaged());
        verify(flightMapper).toDTOList(flightsForMapper.capture());

        assertThat(flightsForMapper.getValue()).zipSatisfy(flightsUntouched, weakEquality);
        assertThat(flightsFromFacade).containsExactlyInAnyOrderElementsOf(desiredFlightDtos);
    }

    @Test
    void getAllFlights_ShouldBeEmpty_IfNoFlightsInService() {
        when(flightService.getAllFlights(Pageable.unpaged())).thenReturn(new ArrayList<>());
        lenient().when(flightMapper.toDTOList(new ArrayList<>())).thenReturn(new ArrayList<>());

        var flightsFromFacade = flightFacade.getAllFlights(Pageable.unpaged());

        verify(flightService).getAllFlights(Pageable.unpaged());
        assertThat(flightsFromFacade).isNotNull().isEmpty();
    }

    @Test
    void getFlightById_ShouldCallServiceAndMapper_IfFlightExists() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var desiredFlightDto = TestDataGenerator.getDefaultFor(FlightDTO.class);
        flight.setId(310L);

        when(flightService.getFlightById(310L)).thenReturn(Optional.of(flight));
        when(flightMapper.toDTO(flight)).thenReturn(desiredFlightDto);

        var flightFromFacade = flightFacade.getFlightById(310L);

        verify(flightService).getFlightById(310L);
        verify(flightMapper).toDTO(flight);
        assertThat(flightFromFacade).contains(desiredFlightDto);
    }

    @Test
    void getFlightById_ShouldReturnEmpty_IfFlightNotInService() {
        when(flightService.getFlightById(310L)).thenReturn(Optional.empty());

        Optional<FlightDTO> flightById = flightFacade.getFlightById(310L);

        verify(flightService).getFlightById(310L);
        verify(flightMapper, never()).toDTO(any());
        assertThat(flightById).isEmpty();
    }

    @Test
    void getFlightByCode_ShouldCallServiceAndMapper_IfFlightExists() {
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var desiredFlightDto = TestDataGenerator.getDefaultFor(FlightDTO.class);

        when(flightService.getFlightByCode(flight.getFlightCode())).thenReturn(Optional.of(flight));
        when(flightMapper.toDTO(flight)).thenReturn(desiredFlightDto);

        var flightFromFacade = flightFacade.getFlightByCode(flight.getFlightCode());

        verify(flightService).getFlightByCode(flight.getFlightCode());
        verify(flightMapper).toDTO(flight);
        assertThat(flightFromFacade).contains(desiredFlightDto);
    }

    @Test
    void getFlightByCode_ShouldReturnEmpty_IfFlightNotInService() {
        var flightCode = TestDataGenerator.getDefaultFor(Flight.class).getFlightCode();

        when(flightService.getFlightByCode(flightCode)).thenReturn(Optional.empty());

        var flightFromFacade = flightFacade.getFlightByCode(flightCode);

        verify(flightService).getFlightByCode(flightCode);
        verify(flightMapper, never()).toDTO(any());
        assertThat(flightFromFacade).isEmpty();
    }

    @Test
    void deleteFlight_ShouldCallServiceMethod() {
        doNothing().when(flightService).deleteFlight(310L);

        flightFacade.deleteFlight(310L);

        verify(flightService).deleteFlight(310L);
    }

    @Test
    void updateFlight_ShouldCallServiceMethod() {

        FlightUpdateDTO updateDTO = new FlightUpdateDTO();
        updateDTO.setId(310L);
        updateDTO.setDepartureAirportId(793L);
        updateDTO.setArrivalAirportId(63L);
        updateDTO.setPlaneId(839L);
        updateDTO.setFlightCode("FL-UPDATED");
        updateDTO.setDepartureTime(LocalDateTime.now().plusHours(2));
        updateDTO.setArrivalTime(LocalDateTime.now().plusHours(4));
        updateDTO.setAvailableSeats(180);
        updateDTO.setStatus(Status.ACTIVE);

        Flight existingFlight = mock(Flight.class);
        Flight updatedFlight = mock(Flight.class);

        when(airportService.getAirportById(63L)).thenReturn(mock(Airport.class));
        when(airportService.getAirportById(793L)).thenReturn(mock(Airport.class));
        when(airplaneService.getAirplaneById(839L)).thenReturn(mock(Airplane.class));
        when(flightService.getFlightById(310L)).thenReturn(Optional.of(existingFlight));
        when(flightService.updateFlight(existingFlight)).thenReturn(updatedFlight);
        when(flightMapper.toDTO(updatedFlight)).thenReturn(new FlightDTO());

        FlightDTO result = flightFacade.updateFlight(updateDTO);

        verify(airportService).getAirportById(63L);
        verify(airportService).getAirportById(793L);
        verify(airplaneService).getAirplaneById(839L);
        verify(flightService).getFlightById(310L);
        verify(flightMapper).updateFlightFromDto(updateDTO, existingFlight);
        verify(flightService).updateFlight(existingFlight);
        verify(flightMapper).toDTO(updatedFlight);
    }

    @Test
    void createFlight_ShouldCallServiceAndMapperMethods() throws IOException {
        var createDto = TestDataGenerator.getDefaultFor(FlightCreateDTO.class);
        var flight = TestDataGenerator.getDefaultFor(Flight.class);
        var flightWithId = TestDataGenerator.getDefaultFor(Flight.class);
        var desiredDto = TestDataGenerator.getDefaultFor(FlightDTO.class);

        flightWithId.setId(310L);

        // Mocks for validation
        when(airportService.getAirportById(createDto.getArrivalAirportId())).thenReturn(new Airport());
        when(airportService.getAirportById(createDto.getDepartureAirportId())).thenReturn(new Airport());
        when(airplaneService.getAirplaneById(createDto.getPlaneId())).thenReturn(new Airplane());

        when(flightMapper.toEntity(createDto)).thenReturn(flight);
        when(flightService.createFlight(flight)).thenReturn(flightWithId);
        when(flightMapper.toDTO(flightWithId)).thenReturn(desiredDto);

        var flightFromFacade = flightFacade.createFlight(createDto);

        verify(flightMapper).toEntity(createDto);
        verify(flightService).createFlight(flight);
        verify(flightMapper).toDTO(flightWithId);

        assertThat(flightFromFacade).isEqualTo(desiredDto);
    }
}
