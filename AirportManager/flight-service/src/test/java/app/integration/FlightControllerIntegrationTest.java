package app.integration;

import app.FlightServiceApplication;
import app.common.TestDataGenerator;
import app.dto.FlightCreateDTO;
import app.dto.FlightUpdateDTO;
import app.entity.Airplane;
import app.entity.Airport;
import app.repository.AirplaneRepository;
import app.repository.AirportRepository;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@AutoConfigureMockMvc(addFilters = false)
@SpringBootTest(classes = FlightServiceApplication.class)
@DirtiesContext(classMode = DirtiesContext.ClassMode.AFTER_EACH_TEST_METHOD)
@ActiveProfiles("test")
public class FlightControllerIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private AirportRepository airportRepository;

    @Autowired
    private AirplaneRepository airplaneRepository;

    @Autowired
    private ObjectMapper objectMapper;

    @BeforeEach
    void setUp() {
        airportRepository.deleteAll();
        airplaneRepository.deleteAll();

        airportRepository.save(new Airport(1L));
        airportRepository.save(new Airport(2L));

        airplaneRepository.save(new Airplane(1L));
    }

    @Test
    void testCreateFlight() throws Exception {
        FlightCreateDTO dto = TestDataGenerator.getDefaultFor(FlightCreateDTO.class);
        dto.setDepartureAirportId(1L);
        dto.setArrivalAirportId(1L);
        dto.setPlaneId(1L);

        String json = objectMapper.writeValueAsString(dto);

        String response = mockMvc.perform(post("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode jsonNode = objectMapper.readTree(response);
        assertThat(jsonNode.has("id")).isTrue();
        assertThat(jsonNode.get("id").asInt()).isPositive();
    }

    private Integer createFlight(int seed) throws Exception {
        FlightCreateDTO dto = TestDataGenerator.instance(seed).of(FlightCreateDTO.class);
        dto.setDepartureAirportId(1L);
        dto.setArrivalAirportId(1L);
        dto.setPlaneId(1L);

        String json = objectMapper.writeValueAsString(dto);

        String response = mockMvc.perform(post("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();

        return objectMapper.readTree(response).get("id").asInt();
    }

    @Test
    void testGetFlightById() throws Exception {
        Integer id = createFlight(0);
        mockMvc.perform(get("/flights/{id}", id))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.id").value(id));
    }

    @Test
    void testGetAllFlights() throws Exception {
        createFlight(0);
        mockMvc.perform(get("/flights"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].flightCode").exists());
    }

    @Test
    void testUpdateFlight() throws Exception {
        Integer id = createFlight(0);

        FlightUpdateDTO dto = TestDataGenerator.getDefaultFor(FlightUpdateDTO.class);
        dto.setId(Long.valueOf(id));
        dto.setArrivalAirportId(1L);
        dto.setDepartureAirportId(1L);
        dto.setPlaneId(1L);
        dto.setPrice(999);

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(put("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isOk());
    }

    @Test
    void testDeleteFlight() throws Exception {
        Integer id = createFlight(0);
        mockMvc.perform(delete("/flights/" + id))
                .andExpect(status().isNoContent());
    }

    @Test
    void testGetNonExistentFlight() throws Exception {
        mockMvc.perform(get("/flights/999999"))
                .andExpect(status().isNotFound());
    }

    @Test
    void testUpdateNonExistentFlight() throws Exception {
        FlightUpdateDTO dto = TestDataGenerator.getDefaultFor(FlightUpdateDTO.class);
        dto.setId(999999L);

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(put("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isNotFound());
    }

    @Test
    void testDeleteNonExistentFlight() throws Exception {
        mockMvc.perform(delete("/flights/999999"))
                .andExpect(status().isNoContent());
    }

    @Test
    void testCreateFlightWithMissingFields() throws Exception {
        String invalidJson = "{}";
        mockMvc.perform(post("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(invalidJson))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testUpdateFlightWithMissingFields() throws Exception {
        String invalidJson = "{}";
        mockMvc.perform(put("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(invalidJson))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testCreateMultipleFlightsAndFetchAll() throws Exception {
        createFlight(0);
        createFlight(1323);
        mockMvc.perform(get("/flights"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[1].flightCode").exists());
    }

    @Test
    void testUpdateFlightPrice() throws Exception {
        Integer id = createFlight(0);
        FlightUpdateDTO dto = TestDataGenerator.getDefaultFor(FlightUpdateDTO.class);
        dto.setArrivalAirportId(1L);
        dto.setDepartureAirportId(1L);
        dto.setPlaneId(1L);
        dto.setId(Long.valueOf(id));
        dto.setPrice(1500);

        String json = objectMapper.writeValueAsString(dto);
        mockMvc.perform(put("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isOk());

        mockMvc.perform(get("/flights/" + id))
                .andExpect(jsonPath("$.price").value(1500));
    }

    @Test
    void testCreateFlightInvalidTimeRange() throws Exception {
        FlightCreateDTO dto = TestDataGenerator.getDefaultFor(FlightCreateDTO.class);
        dto.setDepartureTime(dto.getArrivalTime().plusHours(2));
        dto.setArrivalAirportId(1L);
        dto.setDepartureAirportId(1L);
        dto.setPlaneId(1L);

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(post("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testDeleteAndFetchFlight() throws Exception {
        Integer id = createFlight(0);
        mockMvc.perform(delete("/flights/" + id))
                .andExpect(status().isNoContent());

        mockMvc.perform(get("/flights/" + id))
                .andExpect(status().isNotFound());
    }

    @Test
    void testGetFlightsByStatus() throws Exception {
        createFlight(0);
        mockMvc.perform(get("/flights/byStatus")
                        .param("status", "ACTIVE"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].status").value("ACTIVE"));
    }

    @Test
    void testGetFlightsByAvailableSeats() throws Exception {
        createFlight(0);
        mockMvc.perform(get("/flights/byAvailableSeats")
                        .param("availableSeats", "0"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].availableSeats").exists());
    }

    @Test
    void testGetFlightByCode() throws Exception {
        FlightCreateDTO dto = TestDataGenerator.getDefaultFor(FlightCreateDTO.class);
        dto.setDepartureAirportId(1L);
        dto.setArrivalAirportId(1L);
        dto.setPlaneId(1L);
        dto.setFlightCode("FL123");
        dto.setDepartureTime(dto.getDepartureTime().plusHours(1));
        dto.setArrivalTime(dto.getArrivalTime().plusHours(2));
        String json = objectMapper.writeValueAsString(dto);

        String response = mockMvc.perform(post("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode root = objectMapper.readTree(response);
        String flightCode = root.get("flightCode").asText();

        mockMvc.perform(get("/flights/code/" + flightCode))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.flightCode").value(flightCode));
    }

    @Test
    void testGetCurrentFlights() throws Exception {
        createFlight(0);
        mockMvc.perform(get("/flights/current"))
                .andExpect(status().isOk());
    }

}
