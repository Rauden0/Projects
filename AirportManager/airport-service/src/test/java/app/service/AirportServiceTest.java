package app.service;

import app.airport.entity.Airport;
import app.airport.mapper.AirportEventMapper;
import app.airport.repository.AirportRepository;
import app.airport.service.AirportService;
import app.airport.web.AirportChangeProducer;
import app.exceptions.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;

import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

public class AirportServiceTest {

    @Mock
    private AirportRepository repository;

    @Mock
    private AirportEventMapper eventMapper;

    @Mock
    private AirportChangeProducer changeProducer;


    @InjectMocks
    private AirportService service;

    @BeforeEach
    public void setUp() {
        MockitoAnnotations.openMocks(this);

        doNothing().when(changeProducer).sendAirportChange(any(AirportChangeEvent.class));
    }

    @Test
    public void testGetAllAirports() {
        List<Airport> airports = new ArrayList<>(){
            {
                add(new Airport());
                add(new Airport());
            }
        };
        airports.getFirst().setId(1L);
        airports.getFirst().setName("Heathrow");
        airports.getFirst().setCity("London");
        airports.getFirst().setCountry("UK");
        airports.get(1).setId(2L);
        airports.get(1).setName("JFK");
        airports.get(1).setCity("New York");
        airports.get(1).setCountry("USA");

        when(repository.findAll()).thenReturn(airports);

        List<Airport> result = service.getAllAirports();
        assertNotNull(result);
        assertEquals(2, result.size());
    }

    @Test
    public void testGetAllAirports_Empty() {
        when(repository.findAll()).thenReturn(List.of());

        List<Airport> result = service.getAllAirports();
        assertNotNull(result);
        assertTrue(result.isEmpty());
    }

    @Test
    public void testGetAirportById() {
        Airport airport = new Airport();
        airport.setId(1L);
        airport.setName("Heathrow");
        airport.setCity("London");
        airport.setCountry("UK");
        when(repository.findById(1L)).thenReturn(Optional.of(airport));

        Optional<Airport> result = service.getAirportById(1L);
        assertTrue(result.isPresent());
        assertEquals("Heathrow", result.get().getName());
    }

    @Test
    public void testGetAirportById_NotFound() {
        when(repository.findById(1L)).thenReturn(Optional.empty());

        Optional<Airport> result = service.getAirportById(1L);
        assertFalse(result.isPresent());
    }

    @Test
    public void testCreateAirport() {
        Airport airport = new Airport();
        airport.setId(1L);
        airport.setName("Heathrow");
        airport.setCity("London");

        when(repository.save(airport)).thenReturn(airport);

        Airport result = service.createAirport(airport);
        assertEquals("Heathrow", result.getName());

        verify(changeProducer).sendAirportChange(any(AirportChangeEvent.class));
    }

    @Test
    public void testCreateAirport_Null() {
        when(repository.save(null)).thenThrow(new ResourceNotFoundException("Airport cannot be null"));

        assertThrows(ResourceNotFoundException.class, () -> {
            service.createAirport(null);
        });
    }

    @Test
    public void testDeleteAirport() {
        when(repository.existsById(1L)).thenReturn(true);
        doNothing().when(repository).deleteById(1L);

        service.deleteAirport(1L);

        verify(repository).existsById(1L);
        verify(repository).deleteById(1L);
        verify(changeProducer).sendAirportChange(any(AirportChangeEvent.class));
    }

    @Test
    public void testUpdateAirport_NotFound() {
        when(repository.existsById(1L)).thenReturn(false);

        assertThrows(ResourceNotFoundException.class, () -> {
            service.deleteAirport(1L);
        });

        verify(repository).existsById(1L);
        verify(repository, never()).deleteById(any());
        verify(changeProducer, never()).sendAirportChange(any());
    }

    @Test
    public void testUpdateAirport_Success() {
        Airport airport = new Airport();
        airport.setId(1L);
        airport.setName("Heathrow");

        when(repository.existsById(1L)).thenReturn(true);
        when(repository.save(airport)).thenReturn(airport);

        Airport result = service.updateAirport(airport);
        assertEquals("Heathrow", result.getName());

        verify(repository).existsById(1L);
        verify(repository).save(airport);
    }
}