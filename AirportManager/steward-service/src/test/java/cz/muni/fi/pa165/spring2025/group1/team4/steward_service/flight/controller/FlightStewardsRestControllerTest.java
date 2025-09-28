package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.controller;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.GlobalExceptionHandler;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightStewardsFacade;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightStewardsRestController;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.StewardDto;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.web.PageableHandlerMethodArgumentResolver;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import java.util.List;

import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@ExtendWith(MockitoExtension.class)
class FlightStewardsRestControllerTest {

    private MockMvc mockMvc;

    @Mock
    private FlightStewardsFacade facade;

    @InjectMocks
    private FlightStewardsRestController controller;

    @BeforeEach
    void setup() {
        mockMvc = MockMvcBuilders.standaloneSetup(controller)
                .setCustomArgumentResolvers(new PageableHandlerMethodArgumentResolver())
                .setControllerAdvice(new GlobalExceptionHandler())
                .build();
    }

    @Test
    void getStewardsForFlight_ReturnsList() throws Exception {
        var dto = new StewardDto(1L, "Alice", "Smith");
        when(facade.findAll(42L)).thenReturn(List.of(dto));

        mockMvc.perform(get("/flights/42/stewards"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].id").value(1L))
                .andExpect(jsonPath("$[0].givenName").value("Alice"));
    }

    @Test
    void addStewardToFlight_ReturnsNoContent() throws Exception {
        doNothing().when(facade).addFlightSteward(42L, 99L);

        mockMvc.perform(put("/flights/42/stewards/99"))
                .andExpect(status().isNoContent());
    }

    @Test
    void removeStewardFromFlight_ReturnsNoContent() throws Exception {
        doNothing().when(facade).removeFlightSteward(42L, 99L);

        mockMvc.perform(delete("/flights/42/stewards/99"))
                .andExpect(status().isNoContent());
    }

    @Test
    void addStewardToFlight_WhenStewardBusy_ReturnsConflict409() throws Exception {
        doThrow(new ResourceConflictException("Steward is currently busy"))
                .when(facade).addFlightSteward(42L, 99L);

        mockMvc.perform(put("/flights/42/stewards/99"))
                .andExpect(status().isConflict());
    }

    @Test
    void removeStewardFromFlight_NotFound_Returns404() throws Exception {
        doThrow(new ResourceNotFoundException("Not found"))
                .when(facade).removeFlightSteward(42L, 99L);

        mockMvc.perform(delete("/flights/42/stewards/99"))
                .andExpect(status().isNotFound());
    }
}
