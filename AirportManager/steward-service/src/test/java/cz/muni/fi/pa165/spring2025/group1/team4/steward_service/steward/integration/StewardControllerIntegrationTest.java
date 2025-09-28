package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.integration;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardNewDto;
import jakarta.transaction.Transactional;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;
@Transactional
@SpringBootTest
@AutoConfigureMockMvc(addFilters = false)
class StewardControllerIntegrationTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    private Long createSteward(String firstName, String lastName) throws Exception {
        StewardNewDto dto = new StewardNewDto();
        dto.setGivenName(firstName);
        dto.setFamilyName(lastName);

        String json = objectMapper.writeValueAsString(dto);
        String response = mockMvc.perform(post("/stewards")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();

        JsonNode root = objectMapper.readTree(response);
        return root.get("id").asLong();
    }

    @Test
    void testCreateSteward() throws Exception {
        createSteward("John", "Doe");
    }

    @Test
    void testGetStewardById() throws Exception {
        Long id = createSteward("Jane", "Smith");
        mockMvc.perform(get("/stewards/" + id))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.givenName").value("Jane"));
    }

    @Test
    void testGetAllStewardsPaged() throws Exception {
        createSteward("A", "One");
        createSteward("B", "Two");

        mockMvc.perform(get("/stewards")
                        .param("page", "0")
                        .param("size", "10"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.content.length()").value(2));
    }

    @Test
    void testUpdateSteward() throws Exception {
        Long id = createSteward("Before", "Change");

        StewardDto update = new StewardDto();
        update.setId(id);
        update.setGivenName("After");
        update.setFamilyName("Update");

        String json = objectMapper.writeValueAsString(update);

        mockMvc.perform(patch("/stewards")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.givenName").value("After"));
    }

    @Test
    void testDeleteSteward() throws Exception {
        Long id = createSteward("Delete", "Me");

        mockMvc.perform(delete("/stewards/" + id))
                .andExpect(status().isNoContent());

        mockMvc.perform(get("/stewards/" + id))
                .andExpect(status().isNotFound());
    }

    @Test
    void testCreateStewardInvalidData() throws Exception {
        StewardNewDto dto = new StewardNewDto();
        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(post("/stewards")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testUpdateNonexistentSteward() throws Exception {
        StewardDto dto = new StewardDto();
        dto.setId(9999L);
        dto.setGivenName("Ghost");
        dto.setFamilyName("Person");

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(patch("/stewards")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isNotFound());
    }

    @Test
    void testDeleteNonexistentSteward() throws Exception {
        mockMvc.perform(delete("/stewards/9999"))
                .andExpect(status().isNoContent());
    }

    @Test
    void testUpdateStewardWithMissingFields() throws Exception {
        Long id = createSteward("Will", "Change");

        StewardDto dto = new StewardDto();
        dto.setId(id);

        String json = objectMapper.writeValueAsString(dto);

        mockMvc.perform(patch("/stewards")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(json))
                .andExpect(status().isBadRequest());
    }

    @Test
    void testGetStewardInvalidId() throws Exception {
        mockMvc.perform(get("/stewards/invalid"))
                .andExpect(status().isBadRequest());
    }
}
