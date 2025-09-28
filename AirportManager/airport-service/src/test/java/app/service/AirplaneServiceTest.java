package app.service;

import app.airplane.entity.Airplane;
import app.airplane.entity.AirplaneType;
import app.airplane.mapper.AirplaneEventMapper;
import app.airplane.repository.AirplaneRepository;
import app.airplane.service.AirplaneService;
import app.airplane.web.AirplaneChangeProducer;
import app.exceptions.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
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

public class AirplaneServiceTest {

    @Mock
    private AirplaneEventMapper eventMapper;

    @Mock
    private AirplaneChangeProducer changeProducer;

    @Mock
    private AirplaneRepository repository;

    @InjectMocks
    private AirplaneService service;

    @BeforeEach
    public void setUp() {
        MockitoAnnotations.openMocks(this);

        doNothing().when(changeProducer).sendAirplaneChange(any(AirplaneChangeEvent.class));
    }

    @Test
    public void testGetAllAirplanes() {
        List<Airplane> airplanes = new ArrayList<>(){
            {
                add(new Airplane());
                add(new Airplane());
            }
        };
        airplanes.getFirst().setId(1L);
        airplanes.getFirst().setName("Boeing 747");
        airplanes.getFirst().setType(AirplaneType.COMMERCIAL);
        airplanes.getFirst().setCapacity(300);
        airplanes.get(1).setId(2L);
        airplanes.get(1).setName("Airbus A380");
        airplanes.get(1).setType(AirplaneType.COMMERCIAL);
        airplanes.get(1).setCapacity(500);

        when(repository.findAll()).thenReturn(airplanes);

        List<Airplane> result = service.getAllAirplanes();
        assertNotNull(result);
        assertEquals(2, result.size());
    }

    @Test
    public void testGetAllAirplanes_Empty() {
        when(repository.findAll()).thenReturn(List.of());

        List<Airplane> result = service.getAllAirplanes();
        assertNotNull(result);
        assertTrue(result.isEmpty());
    }

    @Test
    public void testGetAirplaneById() {
        Airplane airplane = new Airplane();
        airplane.setId(1L);
        airplane.setName("Boeing 747");
        airplane.setType(AirplaneType.COMMERCIAL);
        airplane.setCapacity(300);
        when(repository.findById(1L)).thenReturn(Optional.of(airplane));

        Optional<Airplane> result = service.getAirplaneById(1L);
        assertTrue(result.isPresent());
        assertEquals("Boeing 747", result.get().getName());
    }

    @Test
    public void testGetAirplaneById_NotFound() {
        when(repository.findById(1L)).thenReturn(Optional.empty());

        Optional<Airplane> result = service.getAirplaneById(1L);
        assertFalse(result.isPresent());
    }

    @Test
    public void testCreateAirplane() {
        Airplane airplane = new Airplane();
        airplane.setId(1L);
        airplane.setName("Boeing 747");

        when(repository.save(airplane)).thenReturn(airplane);

        Airplane result = service.createAirplane(airplane);
        assertEquals("Boeing 747", result.getName());

        verify(changeProducer).sendAirplaneChange(any(AirplaneChangeEvent.class));
    }

    @Test
    public void testCreateAirplane_Null() {
        when(repository.save(null)).thenThrow(new ResourceNotFoundException("Airplane cannot be null"));

        assertThrows(ResourceNotFoundException.class, () -> {
            service.createAirplane(null);
        });
    }

    @Test
    public void testDeleteAirplane() {
        when(repository.existsById(1L)).thenReturn(true);
        doNothing().when(repository).deleteById(1L);

        service.deleteAirplane(1L);

        verify(repository).existsById(1L);
        verify(repository).deleteById(1L);
        verify(changeProducer).sendAirplaneChange(any(AirplaneChangeEvent.class));
    }

    @Test
    public void testDeleteAirplane_NotFound() {
        when(repository.existsById(1L)).thenReturn(false);

        assertThrows(ResourceNotFoundException.class, () -> {
            service.deleteAirplane(1L);
        });

        verify(repository).existsById(1L);
        verify(repository, never()).deleteById(any());
        verify(changeProducer, never()).sendAirplaneChange(any());
    }

    @Test
    public void testUpdateAirplane_Success() {
        Airplane airplane = new Airplane();
        airplane.setId(1L);
        airplane.setName("Boeing 747");

        when(repository.existsById(1L)).thenReturn(true);
        when(repository.save(airplane)).thenReturn(airplane);

        Airplane result = service.updateAirplane(airplane);
        assertEquals("Boeing 747", result.getName());

        verify(repository).existsById(1L);
        verify(repository).save(airplane);
    }

    @Test
    public void testUpdateAirplane_NotFound() {
        Airplane airplane = new Airplane();
        airplane.setId(1L);

        when(repository.existsById(1L)).thenReturn(false);

        assertThrows(ResourceNotFoundException.class, () -> {
            service.updateAirplane(airplane);
        });

        verify(repository).existsById(1L);
        verify(repository, never()).save(any());
    }

    @Test
    public void testUpdateAirplaneNotFound() {
        Airplane airplane = new Airplane();
        airplane.setId(1L);

        when(repository.findById(1L)).thenReturn(Optional.empty());

        assertThrows(ResourceNotFoundException.class, () -> {
            service.updateAirplane(airplane);
        });
    }
}