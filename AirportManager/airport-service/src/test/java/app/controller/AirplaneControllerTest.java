package app.controller;

import app.airplane.controller.AirplaneController;
import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.entity.AirplaneType;
import app.airplane.facade.AirplaneFacade;
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
class AirplaneControllerTest {

    private final ObjectMapper objectMapper = new ObjectMapper();

    private MockMvc mockMvc;

    @Mock
    private AirplaneFacade facade;

    @InjectMocks
    private AirplaneController controller;

    @BeforeEach
    void setUp() {
        mockMvc = MockMvcBuilders.standaloneSetup(controller)
                .build();
    }

    @Test
    void getAllAirplanes_ReturnsList() throws Exception {
        AirplaneDTO dto1 = new AirplaneDTO();
        AirplaneDTO dto2 = new AirplaneDTO();

        when(facade.getAllAirplanes()).thenReturn(List.of(dto1, dto2));

        mockMvc.perform(get("/airplanes"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.length()").value(2));
    }

    @Test
    void getAirplaneById_ExistingId_ReturnsAirplane() throws Exception {
        AirplaneDTO dto = new AirplaneDTO();

        when(facade.getAirplaneById(1L)).thenReturn(Optional.of(dto));

        mockMvc.perform(get("/airplanes/{id}", 1))
                .andExpect(status().isOk());
    }

    @Test
    void getAirplaneById_NotFound_ReturnsNotFound() throws Exception {
        when(facade.getAirplaneById(999L)).thenReturn(Optional.empty());

        mockMvc.perform(get("/airplanes/{id}", 999))
                .andExpect(status().isNotFound());
    }

    @Test
    void createAirplane_ValidData_ReturnsCreated() throws Exception {
        AirplaneCreateDTO createDTO = new AirplaneCreateDTO();
        createDTO.setCapacity(24);
        createDTO.setName("fs");
        createDTO.setType(AirplaneType.BUSINESS_JET);
        createDTO.setMaximumTravelDistance(1);
        AirplaneDTO createdDTO = new AirplaneDTO();
        createdDTO.setCapacity(24);
        createdDTO.setName("fs");
        createdDTO.setType(AirplaneType.BUSINESS_JET);
        createdDTO.setMaximumTravelDistance(1);

        when(facade.createAirplane(any(AirplaneCreateDTO.class))).thenReturn(createdDTO);

        mockMvc.perform(post("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(createDTO)))
                .andExpect(status().isCreated());
    }

    @Test
    void updateAirplane_ValidData_ReturnsUpdated() throws Exception {


        AirplaneUpdateDTO updateDTO = new AirplaneUpdateDTO();
        AirplaneDTO updatedDTO = new AirplaneDTO();

        when(facade.updateAirplane(any(AirplaneUpdateDTO.class))).thenReturn(updatedDTO);

        mockMvc.perform(put("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(updateDTO)))
                .andExpect(status().isOk());
    }

    @Test
    void deleteAirplane_ExistingId_ReturnsNoContent() throws Exception {
        doNothing().when(facade).deleteAirplane(1L);

        mockMvc.perform(delete("/airplanes/{id}", 1))
                .andExpect(status().isNoContent());

        verify(facade).deleteAirplane(1L);
    }

    @Test
    void getAllAirplanes_EmptyList_ReturnsEmptyArray() throws Exception {
        when(facade.getAllAirplanes()).thenReturn(List.of());

        mockMvc.perform(get("/airplanes"))
                .andExpect(status().isOk())
                .andExpect(content().json("[]"));
    }
}
