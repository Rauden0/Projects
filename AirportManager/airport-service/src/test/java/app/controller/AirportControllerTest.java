package app.controller;

import app.airport.controller.AirportController;
import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.facade.AirportFacade;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import java.util.List;
import java.util.Optional;

import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@ExtendWith(MockitoExtension.class)
class AirportControllerTest {

    private final ObjectMapper objectMapper = new ObjectMapper();

    private MockMvc mockMvc;

    @Mock
    private AirportFacade facade;

    @InjectMocks
    private AirportController controller;

    @BeforeEach
    void setUp() {
        mockMvc = MockMvcBuilders.standaloneSetup(controller).build();
    }

    @Test
    void getAllAirports_ReturnsList() throws Exception {
        AirportDTO dto1 = new AirportDTO();
        AirportDTO dto2 = new AirportDTO();

        when(facade.getAllAirports()).thenReturn(List.of(dto1, dto2));

        mockMvc.perform(get("/airports"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.length()").value(2));
    }

    @Test
    void getAirportById_ExistingId_ReturnsAirport() throws Exception {
        AirportDTO dto = new AirportDTO();

        when(facade.getAirportById(1L)).thenReturn(Optional.of(dto));

        mockMvc.perform(get("/airports/{id}", 1))
                .andExpect(status().isOk());
    }

    @Test
    void getAirportById_NotFound_ReturnsNotFound() throws Exception {
        when(facade.getAirportById(999L)).thenReturn(Optional.empty());

        mockMvc.perform(get("/airports/{id}", 999))
                .andExpect(status().isNotFound());
    }

    @Test
    void createAirport_ValidData_ReturnsCreated() throws Exception {
        AirportCreateDTO createDTO = new AirportCreateDTO();
        createDTO.setCountry("as");
        createDTO.setName("fs");
        createDTO.setCity("afss");
        createDTO.setCapacity(30);
        AirportDTO createdDTO = new AirportDTO();
        createdDTO.setCountry("as");
        createdDTO.setName("fs");
        createdDTO.setCity("afss");

        when(facade.createAirport(any(AirportCreateDTO.class))).thenReturn(createdDTO);

        mockMvc.perform(post("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(createDTO)))
                .andExpect(status().isCreated());
    }

    @Test
    void updateAirport_ValidData_ReturnsUpdated() throws Exception {
        AirportUpdateDTO updateDTO = new AirportUpdateDTO();
        updateDTO.setId(1L);
        updateDTO.setCountry("as");
        updateDTO.setName("fs");
        updateDTO.setCity("afss");
        AirportDTO updatedDTO = new AirportDTO();
        updatedDTO.setId(1L);
        updatedDTO.setCountry("as");
        updatedDTO.setName("fs");
        updatedDTO.setCity("afss");

        when(facade.updateAirport(any(AirportUpdateDTO.class))).thenReturn(updatedDTO);

        mockMvc.perform(put("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(updateDTO)))
                .andExpect(status().isOk());
    }

    @Test
    void deleteAirport_ExistingId_ReturnsNoContent() throws Exception {
        doNothing().when(facade).deleteAirport(1L);

        mockMvc.perform(delete("/airports/{id}", 1))
                .andExpect(status().isNoContent());

        verify(facade).deleteAirport(1L);
    }

    @Test
    void getAllAirports_EmptyList_ReturnsEmptyArray() throws Exception {
        when(facade.getAllAirports()).thenReturn(List.of());

        mockMvc.perform(get("/airports"))
                .andExpect(status().isOk())
                .andExpect(content().json("[]"));
    }

    @Test
    void createAirport_NullBody_ReturnsBadRequest() throws Exception {
        mockMvc.perform(post("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("null"))
                .andExpect(status().isBadRequest());
    }

}
