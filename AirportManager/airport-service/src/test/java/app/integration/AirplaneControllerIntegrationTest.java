package app.integration;

import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.entity.AirplaneType;
import app.airplane.web.AirplaneChangeProducer;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.transaction.Transactional;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc(addFilters = false)
@Transactional
class AirplaneControllerIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockitoBean
    private AirplaneChangeProducer changeProducer;

    private Long createAirplane(String name, AirplaneType type, Integer capacity, Integer maxTravel) throws Exception {
        AirplaneCreateDTO dto = new AirplaneCreateDTO();
        dto.setName(name);
        dto.setType(type);
        dto.setCapacity(capacity);
        dto.setMaximumTravelDistance(maxTravel);

        String json = objectMapper.writeValueAsString(dto);

        String response = mockMvc.perform(post("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode jsonNode = objectMapper.readTree(response);
        return jsonNode.get("id").asLong();
    }

    @Test
    void testCreateValidAirplane() throws Exception {
        createAirplane("Boeing 737", AirplaneType.CARGO, 24, 1);
    }

    @Test
    void testCreateInvalidAirplane() throws Exception {
        AirplaneCreateDTO dto = new AirplaneCreateDTO();
        dto.setType(AirplaneType.BUSINESS_JET); // Missing name

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(post("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testGetAllAirplanes() throws Exception {
        createAirplane("Boeing 747", AirplaneType.CARGO, 24, 1);

        mockMvc.perform(get("/airplanes"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].name").value("Boeing 747"));
    }

    @Test
    void testGetAirplaneById() throws Exception {
        Long id = createAirplane("Airbus A320", AirplaneType.CARGO, 24, 1);
        mockMvc.perform(get("/airplanes/{id}", id))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.name").value("Airbus A320"));
    }

    @Test
    void testGetNonExistentAirplane() throws Exception {
        mockMvc.perform(get("/airplanes/99999"))
                .andExpect(status().isNotFound());
    }

    @Test
    void testUpdateAirplane() throws Exception {
        Long id = createAirplane("Antonov", AirplaneType.CARGO, 24, 1);

        AirplaneUpdateDTO update = new AirplaneUpdateDTO();
        update.setName("Antonov Updated");
        update.setType(AirplaneType.CARGO);
        update.setId(id);
        String json = objectMapper.writeValueAsString(update);

        mockMvc.perform(put("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isOk());
    }

    @Test
    void testUpdateNonExistentAirplane() throws Exception {
        AirplaneUpdateDTO update = new AirplaneUpdateDTO();
        update.setName("Ghost Plane");
        update.setType(AirplaneType.CARGO);
        update.setId(99999L);

        String json = objectMapper.writeValueAsString(update);

        mockMvc.perform(put("/airplanes")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isNotFound());
    }

    @Test
    void testDeleteAirplane() throws Exception {
        Long id = createAirplane("Test Plane", AirplaneType.CARGO, 24, 1);

        mockMvc.perform(delete("/airplanes/{id}", id))
                .andExpect(status().isNoContent());
    }

    @Test
    void testDeleteNonExistentAirplane() throws Exception {
        mockMvc.perform(delete("/airplanes/99999"))
                .andExpect(status().isNotFound());
    }
}
