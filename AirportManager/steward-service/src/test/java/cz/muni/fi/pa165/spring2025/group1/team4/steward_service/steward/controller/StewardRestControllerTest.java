package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.GlobalExceptionHandler;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.*;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.web.PageableHandlerMethodArgumentResolver;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import java.util.List;
import java.util.Optional;

import static cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.TestData.createStewardDto;
import static cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.TestData.createStewardNewDto;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ExtendWith(MockitoExtension.class)
class StewardRestControllerTest {

    private final ObjectMapper objectMapper = new ObjectMapper();
    private MockMvc mockMvc;
    @Mock
    private StewardFacade stewardFacade;
    @InjectMocks
    private StewardRestController stewardRestController;

    @BeforeEach
    void setup() {
        mockMvc = MockMvcBuilders.standaloneSetup(stewardRestController).setCustomArgumentResolvers(new PageableHandlerMethodArgumentResolver()).setControllerAdvice(new GlobalExceptionHandler()).build();
    }

    @Test
    void getStewardById_ExistingId_ReturnsSteward() throws Exception {
        StewardDto dto = createStewardDto(1L);
        when(stewardFacade.findById(1L)).thenReturn(Optional.of(dto));

        mockMvc.perform(get("/stewards/{id}", 1L)).andExpect(status().isOk()).andExpect(jsonPath("$.id").value(1L)).andExpect(jsonPath("$.givenName").value("Tonda")).andExpect(jsonPath("$.familyName").value("Tondovicz"));
    }

    @Test
    void getStewardById_NonExistingId_ReturnsNotFound() throws Exception {
        when(stewardFacade.findById(999L)).thenReturn(Optional.empty());

        mockMvc.perform(get("/stewards/{id}", 999L)).andExpect(status().isNotFound());
    }

    @Test
    void getAllStewards_ReturnsPageOfStewards() throws Exception {
        StewardDto stewardDto = TestData.createStewardDto(1L);
        List<StewardDto> content = List.of(stewardDto);

        Pageable pageable = PageRequest.of(0, 10);
        Page<StewardDto> page = new PageImpl<>(content, pageable, content.size());

        when(stewardFacade.findAllStewards(any(Pageable.class))).thenReturn(page);

        mockMvc.perform(get("/stewards").param("page", "0").param("size", "10")).andExpect(status().isOk()).andExpect(jsonPath("$.content[0].id").value(1L)).andExpect(jsonPath("$.totalElements").value(1));
    }

    @Test
    void createSteward_ValidData_ReturnsCreatedSteward() throws Exception {
        StewardNewDto newDto = createStewardNewDto();
        StewardDto createdDto = createStewardDto(1L);

        when(stewardFacade.createSteward(any(StewardNewDto.class))).thenReturn(createdDto);

        mockMvc.perform(post("/stewards").contentType(MediaType.APPLICATION_JSON).content(objectMapper.writeValueAsString(newDto))).andExpect(status().isCreated()).andExpect(jsonPath("$.id").value(1L));
    }

    @Test
    void createSteward_InvalidData_ReturnsBadRequest() throws Exception {
        StewardNewDto invalidDto = createStewardNewDto();
        invalidDto.setGivenName("");

        mockMvc.perform(post("/stewards").contentType(MediaType.APPLICATION_JSON).content(objectMapper.writeValueAsString(invalidDto))).andExpect(status().isBadRequest());
    }

    @Test
    void updateSteward_ValidData_ReturnsUpdatedSteward() throws Exception {
        StewardDto updateDto = createStewardDto(1L);
        when(stewardFacade.updateSteward(any(StewardDto.class))).thenReturn(updateDto);

        mockMvc.perform(patch("/stewards").contentType(MediaType.APPLICATION_JSON).content(objectMapper.writeValueAsString(updateDto))).andExpect(status().isOk()).andExpect(jsonPath("$.id").value(1L));
    }

    @Test
    void deleteSteward_ExistingId_ReturnsNoContent() throws Exception {
        doNothing().when(stewardFacade).deleteSteward(1L);

        mockMvc.perform(delete("/stewards/{id}", 1L)).andExpect(status().isNoContent());

        verify(stewardFacade).deleteSteward(1L);
    }
}