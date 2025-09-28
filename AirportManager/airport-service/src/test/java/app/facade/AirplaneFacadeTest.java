package app.facade;

import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.entity.Airplane;
import app.airplane.facade.AirplaneFacade;
import app.airplane.mapper.AirplaneMapper;
import app.airplane.service.AirplaneService;
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

public class AirplaneFacadeTest {

    @Mock
    private AirplaneService service;

    @Mock
    private AirplaneMapper mapper;

    @InjectMocks
    private AirplaneFacade facade;

    @BeforeEach
    public void setUp() {
        MockitoAnnotations.openMocks(this);
    }

    @Test
    public void testGetAllAirplanes() {
        List<Airplane> airplanes = List.of(new Airplane(), new Airplane());
        List<AirplaneDTO> airplaneDTOs = List.of(new AirplaneDTO(), new AirplaneDTO());

        when(service.getAllAirplanes()).thenReturn(airplanes);
        when(mapper.toDTOList(airplanes)).thenReturn(airplaneDTOs);

        List<AirplaneDTO> result = facade.getAllAirplanes();

        assertNotNull(result);
        assertEquals(2, result.size());
        verify(service, times(1)).getAllAirplanes();
        verify(mapper, times(1)).toDTOList(airplanes);
    }

    @Test
    public void testGetAirplaneById() {
        Airplane airplane = new Airplane();
        AirplaneDTO airplaneDTO = new AirplaneDTO();

        when(service.getAirplaneById(1L)).thenReturn(Optional.of(airplane));
        when(mapper.toDTO(airplane)).thenReturn(airplaneDTO);

        Optional<AirplaneDTO> result = facade.getAirplaneById(1L);

        assertTrue(result.isPresent());
        assertEquals(airplaneDTO, result.get());
        verify(service, times(1)).getAirplaneById(1L);
        verify(mapper, times(1)).toDTO(airplane);
    }

    @Test
    public void testGetAirplaneById_NotFound() {
        when(service.getAirplaneById(1L)).thenReturn(Optional.empty());

        Optional<AirplaneDTO> result = facade.getAirplaneById(1L);

        assertFalse(result.isPresent());
        verify(service, times(1)).getAirplaneById(1L);
        verifyNoInteractions(mapper);
    }

    @Test
    public void testDeleteAirplane() {
        when(service.existsById(1L)).thenReturn(true);
        doNothing().when(service).deleteAirplane(1L);

        facade.deleteAirplane(1L);

        verify(service, times(1)).existsById(1L);
        verify(service, times(1)).deleteAirplane(1L);
        verifyNoInteractions(mapper);
    }

    @Test
    public void testUpdateAirplane() {
        AirplaneUpdateDTO updateDTO = new AirplaneUpdateDTO();
        updateDTO.setId(1L);
        Airplane airplane = new Airplane();
        airplane.setId(1L);
        Airplane existingAirplane = new Airplane();
        existingAirplane.setId(1L);
        AirplaneDTO airplaneDTO = new AirplaneDTO();
        airplaneDTO.setId(1L);

        when(service.getAirplaneById(1L)).thenReturn(Optional.of(existingAirplane));
        when(mapper.toEntity(updateDTO)).thenReturn(airplane);
        when(service.updateAirplane(any(Airplane.class))).thenReturn(airplane);
        when(mapper.toDTO(airplane)).thenReturn(airplaneDTO);

        AirplaneDTO result = facade.updateAirplane(updateDTO);

        assertNotNull(result);
        assertEquals(airplaneDTO, result);

        verify(service, times(1)).getAirplaneById(1L);
        verify(mapper, times(1)).updateAirplaneFromDto(updateDTO, existingAirplane);
        verify(service, times(1)).updateAirplane(existingAirplane);
        verify(mapper, times(1)).toDTO(airplane);
    }

    @Test
    public void testCreateAirplane() {
        AirplaneCreateDTO createDTO = new AirplaneCreateDTO();
        Airplane airplane = new Airplane();
        AirplaneDTO airplaneDTO = new AirplaneDTO();

        when(mapper.toEntity(createDTO)).thenReturn(airplane);
        when(service.createAirplane(airplane)).thenReturn(airplane);
        when(mapper.toDTO(airplane)).thenReturn(airplaneDTO);

        AirplaneDTO result = facade.createAirplane(createDTO);

        assertNotNull(result);
        assertEquals(airplaneDTO, result);
        verify(mapper, times(1)).toEntity(createDTO);
        verify(service, times(1)).createAirplane(airplane);
        verify(mapper, times(1)).toDTO(airplane);
    }

    @Test
    public void testGetAllAirplanes_Empty() {
        when(service.getAllAirplanes()).thenReturn(List.of());
        when(mapper.toDTOList(List.of())).thenReturn(List.of());

        List<AirplaneDTO> result = facade.getAllAirplanes();

        assertNotNull(result);
        assertTrue(result.isEmpty());
        verify(service, times(1)).getAllAirplanes();
        verify(mapper, times(1)).toDTOList(List.of());
    }

    @Test
    public void testUpdateAirplane_NotFound() {
        AirplaneUpdateDTO updateDTO = new AirplaneUpdateDTO();
        updateDTO.setId(1L);

        when(service.getAirplaneById(1L)).thenReturn(Optional.empty());

        assertThrows(ResourceNotFoundException.class, () -> {
            facade.updateAirplane(updateDTO);
        });

        verify(service, times(1)).getAirplaneById(1L);
        verifyNoInteractions(mapper);
        verify(service, never()).updateAirplane(any());
    }

    @Test
    public void testCreateAirplane_Null() {
        when(mapper.toEntity((AirplaneCreateDTO) null)).thenThrow(new IllegalArgumentException("Airplane cannot be null"));

        assertThrows(IllegalArgumentException.class, () -> {
            facade.createAirplane(null);
        });

        verify(mapper, times(1)).toEntity((AirplaneCreateDTO) null);
        verifyNoInteractions(service);
    }
}
