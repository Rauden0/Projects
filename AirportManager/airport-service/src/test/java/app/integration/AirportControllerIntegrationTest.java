package app.integration;

import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.web.AirportChangeProducer;
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
class AirportControllerIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockitoBean
    private AirportChangeProducer changeProducer;

    private Long createAirport(String name, String city, String country, Integer capacity) throws Exception {
        AirportCreateDTO dto = new AirportCreateDTO();
        dto.setName(name);
        dto.setCity(city);
        dto.setCountry(country);
        dto.setCapacity(capacity);

        String json = objectMapper.writeValueAsString(dto);

        String response = mockMvc.perform(post("/airports")
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
    void testCreateValidAirport() throws Exception {
        createAirport("Brno Airport", "Brno", "Czech Republic", 80);
    }

    @Test
    void testCreateInvalidAirport() throws Exception {
        AirportCreateDTO dto = new AirportCreateDTO();
        dto.setCity("Brno"); // Missing name and country

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(post("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testGetAllAirports() throws Exception {
        createAirport("Vienna International", "Vienna", "Austria", 80);

        mockMvc.perform(get("/airports"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].name").value("Vienna International"));
    }

    @Test
    void testGetAirportById() throws Exception {
        Long id = createAirport("Schiphol", "Amsterdam", "Netherlands", 80);

        mockMvc.perform(get("/airports/{id}", id))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.name").value("Schiphol"));
    }

    @Test
    void testGetNonExistentAirport() throws Exception {
        mockMvc.perform(get("/airports/99999"))
                .andExpect(status().isNotFound());
    }

    @Test
    void testUpdateAirport() throws Exception {
        Long id = createAirport("Old Name", "City", "Country", 80);

        AirportUpdateDTO update = new AirportUpdateDTO();
        update.setId(id);
        update.setName("New Name");
        update.setCity("New City");
        update.setCountry("New Country");

        String json = objectMapper.writeValueAsString(update);

        mockMvc.perform(put("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isOk());
    }

    @Test
    void testUpdateNonExistentAirport() throws Exception {
        AirportUpdateDTO dto = new AirportUpdateDTO();
        dto.setId(99999L);
        dto.setName("Ghost");
        dto.setCity("Nowhere");
        dto.setCountry("N/A");

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(put("/airports")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isNotFound());
    }

    @Test
    void testDeleteAirport() throws Exception {
        Long id = createAirport("Temp Airport", "City", "Country", 80);

        mockMvc.perform(delete("/airports/{id}", id))
                .andExpect(status().isNoContent());
    }

    @Test
    void testDeleteNonExistentAirport() throws Exception {
        mockMvc.perform(delete("/airports/99999"))
                .andExpect(status().isNotFound());
    }
}
