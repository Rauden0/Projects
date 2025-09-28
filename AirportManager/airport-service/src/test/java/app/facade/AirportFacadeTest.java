package app.facade;

import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.entity.Airport;
import app.airport.facade.AirportFacade;
import app.airport.mapper.AirportMapper;
import app.airport.service.AirportService;
import app.exceptions.ResourceNotFoundException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;

import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

public class AirportFacadeTest {

    @Mock
    private AirportService service;

    @Mock
    private AirportMapper mapper;

    @InjectMocks
    private AirportFacade facade;

    @BeforeEach
    public void setUp() {
        MockitoAnnotations.openMocks(this);
    }

    @Test
    public void testGetAllAirports() {
        List<Airport> airports = List.of(new Airport(), new Airport());
        List<AirportDTO> airportDTOs = List.of(new AirportDTO(), new AirportDTO());

        when(service.getAllAirports()).thenReturn(airports);
        when(mapper.toDTOList(airports)).thenReturn(airportDTOs);

        List<AirportDTO> result = facade.getAllAirports();
        assertNotNull(result);
        assertEquals(2, result.size());

        verify(service, times(1)).getAllAirports();
        verify(mapper, times(1)).toDTOList(airports);
    }

    @Test
    public void testGetAirportById() {
        Airport airport = new Airport();
        AirportDTO airportDTO = new AirportDTO();

        when(service.getAirportById(1L)).thenReturn(Optional.of(airport));
        when(mapper.toDTO(airport)).thenReturn(airportDTO);

        Optional<AirportDTO> result = facade.getAirportById(1L);
        assertTrue(result.isPresent());
        assertEquals(airportDTO, result.get());

        verify(service, times(1)).getAirportById(1L);
        verify(mapper, times(1)).toDTO(airport);
    }

    @Test
    public void testGetAirportById_NotFound() {
        when(service.getAirportById(1L)).thenReturn(Optional.empty());

        Optional<AirportDTO> result = facade.getAirportById(1L);
        assertFalse(result.isPresent());

        verify(service, times(1)).getAirportById(1L);
        verify(mapper, never()).toDTO(any());
    }

    @Test
    public void testDeleteAirport_Success() {
        when(service.existsById(1L)).thenReturn(true);
        doNothing().when(service).deleteAirport(1L);

        facade.deleteAirport(1L);

        verify(service).existsById(1L);
        verify(service).deleteAirport(1L);
    }

    @Test
    public void testDeleteAirport_NotFound() {
        when(service.existsById(1L)).thenReturn(false);

        assertThrows(ResourceNotFoundException.class, () -> {
            facade.deleteAirport(1L);
        });

        verify(service).existsById(1L);
        verify(service, never()).deleteAirport(any());
    }

    @Test
    public void testUpdateAirport() {
        AirportUpdateDTO updateDTO = new AirportUpdateDTO();
        updateDTO.setId(1L);
        Airport airport = new Airport();
        airport.setId(1L);
        Airport existingAirport = new Airport();
        existingAirport.setId(1L);
        AirportDTO airportDTO = new AirportDTO();
        airportDTO.setId(1L);

        when(service.getAirportById(1L)).thenReturn(Optional.of(existingAirport));
        when(mapper.toEntity(updateDTO)).thenReturn(airport);
        when(service.updateAirport(any(Airport.class))).thenReturn(airport);
        when(mapper.toDTO(airport)).thenReturn(airportDTO);

        AirportDTO result = facade.updateAirport(updateDTO);

        assertNotNull(result);
        assertEquals(airportDTO, result);

        verify(service, times(1)).getAirportById(1L);
        verify(mapper, times(1)).updateAirportFromDto(updateDTO, existingAirport);
        verify(service, times(1)).updateAirport(existingAirport);
        verify(mapper, times(1)).toDTO(airport);
    }

    @Test
    public void testCreateAirport() {
        AirportCreateDTO createDTO = new AirportCreateDTO();
        Airport airport = new Airport();
        AirportDTO airportDTO = new AirportDTO();

        when(mapper.toEntity(createDTO)).thenReturn(airport);
        when(service.createAirport(airport)).thenReturn(airport);
        when(mapper.toDTO(airport)).thenReturn(airportDTO);

        AirportDTO result = facade.createAirport(createDTO);
        assertNotNull(result);
        assertEquals(airportDTO, result);

        verify(mapper, times(1)).toEntity(createDTO);
        verify(service, times(1)).createAirport(airport);
        verify(mapper, times(1)).toDTO(airport);
    }

    @Test
    public void testGetAllAirports_Empty() {
        when(service.getAllAirports()).thenReturn(List.of());
        when(mapper.toDTOList(List.of())).thenReturn(List.of());

        List<AirportDTO> result = facade.getAllAirports();
        assertNotNull(result);
        assertTrue(result.isEmpty());

        verify(service, times(1)).getAllAirports();
        verify(mapper, times(1)).toDTOList(List.of());
    }

    @Test
    public void testUpdateAirport_NotFound() {
        AirportUpdateDTO updateDTO = new AirportUpdateDTO();
        updateDTO.setId(1L);

        when(service.getAirportById(1L)).thenReturn(Optional.empty());

        assertThrows(ResourceNotFoundException.class, () -> {
            facade.updateAirport(updateDTO);
        });

        verify(service, times(1)).getAirportById(1L);
        verifyNoMoreInteractions(service);
        verifyNoInteractions(mapper);
    }

    @Test
    public void testCreateAirport_Null() {
        when(mapper.toEntity((AirportCreateDTO) null)).thenThrow(new IllegalArgumentException("Airport cannot be null"));

        assertThrows(IllegalArgumentException.class, () -> facade.createAirport(null));

        verify(mapper, times(1)).toEntity((AirportCreateDTO) null);
        verify(service, never()).createAirport(any());
        verify(mapper, never()).toDTO(any());
    }
}