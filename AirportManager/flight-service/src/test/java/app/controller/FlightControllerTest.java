package app.controller;

import app.FlightServiceApplication;
import app.common.TestDataGenerator;
import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.facade.FlightFacade;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.data.domain.Pageable;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.ResultActions;

import java.io.UnsupportedEncodingException;
import java.nio.charset.StandardCharsets;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest(classes = { FlightServiceApplication.class })
@AutoConfigureMockMvc(addFilters = false)
public class FlightControllerTest {
    @Autowired
    private MockMvc mvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockitoBean
    private FlightFacade facade;

    @Test
    void getFlights_ReturnsFlightsFromFacade() throws Exception {
        var flights = TestDataGenerator.first(8).instancesOf(FlightDTO.class);

        when(facade.getAllFlights(any(Pageable.class))).thenReturn(flights);

        String responseJson = getResponseJson(mvc.perform(
                        get("/flights")
                                .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isOk()));

        verify(facade).getAllFlights(any(Pageable.class));

        assertThat(responseJson)
                .isEqualToIgnoringWhitespace(objectMapper.writeValueAsString(flights));
    }


    @Test
    void getFlightByCode_ReturnsFlightFromFacade_IfFlightExists()
            throws Exception {
        var flightDto = TestDataGenerator.getDefaultFor(FlightDTO.class);
        String flightCode = flightDto.getFlightCode();

        when(facade.getFlightByCode(flightCode)).thenReturn(Optional.of(flightDto));

        String responseJson = getResponseJson(mvc.perform(
                get("/flights/code/{code}", flightCode)
                        .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isOk()));

        verify(facade).getFlightByCode(flightCode);

        assertThat(responseJson)
                .isEqualToIgnoringWhitespace(objectMapper.writeValueAsString(flightDto));

    }

    @Test
    void getFlightByCode_ReturnsNotFound_IfFlightDoesNotExist()
            throws UnsupportedEncodingException, Exception {
        String flightCode = TestDataGenerator.getDefaultFor(FlightDTO.class).getFlightCode();

        when(facade.getFlightByCode(flightCode)).thenReturn(Optional.empty());

        mvc.perform(get("/flights/code/{code}", flightCode)
                .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isNotFound());

        verify(facade).getFlightByCode(flightCode);
    }

    @Test
    void getFlightById_ReturnsFlightFromFacade_IfFlightExists()
            throws UnsupportedEncodingException, Exception {
        FlightDTO flightDto = TestDataGenerator.getDefaultFor(FlightDTO.class);

        when(facade.getFlightById(310L)).thenReturn(Optional.of(flightDto));

        String responseJson = getResponseJson(mvc.perform(
                get("/flights/{id}", 310)
                        .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isOk()));

        verify(facade).getFlightById(310L);

        assertThat(responseJson)
                .isEqualToIgnoringWhitespace(objectMapper.writeValueAsString(flightDto));

    }

    @Test
    void getFlightById_ReturnsNotFound_IfFlightDoesNotExist()
            throws UnsupportedEncodingException, Exception {
        when(facade.getFlightById(310L)).thenReturn(Optional.empty());

        mvc.perform(get("/flights/{id}", 310)
                .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isNotFound());

        verify(facade).getFlightById(310L);
    }

    @Test
    void createFlight_Succeeds_OnValidData() throws Exception {
        var createDto = TestDataGenerator.getDefaultFor(FlightCreateDTO.class);
        var dto = TestDataGenerator.getDefaultFor(FlightDTO.class);

        when(facade.createFlight(createDto)).thenReturn(dto);

        String responseJson = getResponseJson(mvc.perform(
                post("/flights")
                        .contentType(MediaType.APPLICATION_JSON_VALUE)
                        .content(objectMapper.writeValueAsString(createDto))
                        .accept(MediaType.APPLICATION_JSON_VALUE))
                .andExpect(status().isCreated()));

        verify(facade).createFlight(createDto);

        assertThat(responseJson)
                .isEqualToIgnoringWhitespace(objectMapper.writeValueAsString(dto));
    }

    @Test
    void deleteFlight_CallsFacadeMethodAndReturnsNoContent() throws Exception {
        doNothing().when(facade).deleteFlight(310L);

        mvc.perform(delete("/flights/{id}", 310)).andExpect(status().isNoContent());

        verify(facade).deleteFlight(310L);
    }

    @Test
    void updateFlight_CallsFacadeMethodAndReturnsOk() throws Exception {
        var updateDto = TestDataGenerator.getDefaultFor(FlightUpdateDTO.class);
        updateDto.setId(310L);
        var dto = TestDataGenerator.getDefaultFor(FlightDTO.class);

        when(facade.updateFlight(updateDto)).thenReturn(dto);

        String responseJson = getResponseJson(mvc.perform(
                put("/flights")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(updateDto)))
                .andExpect(status().isOk()));

        verify(facade).updateFlight(updateDto);

        assertThat(responseJson).isEqualToIgnoringWhitespace(objectMapper.writeValueAsString(dto));

    }

    private String getResponseJson(ResultActions response) throws UnsupportedEncodingException {
        return response
                .andReturn()
                .getResponse()
                .getContentAsString(StandardCharsets.UTF_8);
    }
}
