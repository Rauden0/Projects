package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.integration;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.StewardServiceApplication;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;

import java.time.LocalDateTime;
import java.util.Set;

import static java.util.concurrent.TimeUnit.SECONDS;
import static org.assertj.core.api.Assertions.assertThat;
import static org.awaitility.Awaitility.await;
import static org.hamcrest.Matchers.hasSize;
import static org.springframework.test.annotation.DirtiesContext.ClassMode.AFTER_EACH_TEST_METHOD;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest(classes = StewardServiceApplication.class)
@DirtiesContext(classMode = AFTER_EACH_TEST_METHOD)
@ActiveProfiles("test")
@AutoConfigureMockMvc(addFilters = false)
class FlightIntegrationTest {

    @Autowired
    private MockMvc mockMvc;
    @Autowired
    private FlightRepository flightRepository;
    @Autowired
    private StewardRepository stewardRepository;
    @Autowired
    private JmsTemplate jmsTemplate;

    private static Flight createFlight(String departureTime, long durationHours, long id) {
        LocalDateTime departure = LocalDateTime.parse(departureTime);
        LocalDateTime arrival = departure.plusHours(durationHours);
        Flight flight = Flight.spanning(departure, arrival);
        flight.setId(id);
        return flight;
    }

    @Test
    void addStewardToFlight_AssignsSteward() throws Exception {
        Flight flight = flightRepository.save(createFlight("2023-10-01T10:00:00", 2, 1));
        Steward steward = stewardRepository.save(Steward.named("John", "Doe"));

        mockMvc.perform(put("/flights/" + flight.getId() + "/stewards/" + steward.getId()))
                .andExpect(status().isNoContent());

        assertThat(flightRepository.findById(flight.getId()).get().getStewards()).containsOnly(steward);
    }

    @Test
    void removeStewardFromFlight_UnassignsSteward() throws Exception {
        Steward steward = stewardRepository.save(Steward.named("Jane", "Doe"));
        Flight flight = createFlight("2023-10-01T10:00:00", 2, 1);
        flight.setStewards(Set.of(steward));
        flight = flightRepository.save(flight);

        mockMvc.perform(delete("/flights/" + flight.getId() + "/stewards/" + steward.getId()))
                .andExpect(status().isNoContent());

        assertThat(flightRepository.findById(flight.getId()).get().getStewards()).doesNotContain(steward);
    }

    @Test
    void addBusyStewardToFlight_ThrowsConflict() throws Exception {
        Steward steward = stewardRepository.save(Steward.named("Busy", "Bee"));

        Flight flight1 = createFlight("2023-10-01T10:00:00", 2, 1);
        flight1.setStewards(Set.of(steward));
        flightRepository.save(flight1);

        LocalDateTime departure2 = LocalDateTime.parse("2023-10-01T11:30:00");
        LocalDateTime arrival2 = departure2.plusHours(2);
        Flight flight2Presave = Flight.spanning(departure2, arrival2);
        flight2Presave.setId(2L);
        Flight flight2 = flightRepository.save(flight2Presave);

        mockMvc.perform(put("/flights/" + flight2.getId() + "/stewards/" + steward.getId()))
                .andExpect(status().isConflict());
    }

    @Test
    void getStewardsForFlight_ReturnsList() throws Exception {
        Steward steward1 = stewardRepository.save(Steward.named("Anna", "Smith"));
        Steward steward2 = stewardRepository.save(Steward.named("Tom", "Jones"));
        Flight flight = createFlight("2023-10-01T10:00:00", 2, 1);
        flight.setStewards(Set.of(steward1, steward2));
        flight = flightRepository.save(flight);

        mockMvc.perform(get("/flights/" + flight.getId() + "/stewards"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$", hasSize(2)));
    }

    @Test
    void flightDeletedEvent_DeletesFlight() {
        Flight flight = createFlight("2023-10-01T10:00:00", 2, 1);

        Steward steward = stewardRepository.save(Steward.named("John", "Doe"));
        flight.getStewards().add(steward);

        Flight saved = flightRepository.save(flight);

        FlightDto dto = FlightDto.builder()
                .id(saved.getId())
                .departureTime(saved.getDepartureTime())
                .arrivalTime(saved.getArrivalTime())
                .build();

        FlightChangeEvent event = new FlightChangeEvent(ChangeType.DELETED, dto);

        jmsTemplate.convertAndSend("flight-changes-for-steward-service", event);

        await().atMost(2, SECONDS).until(() -> !flightRepository.existsById(saved.getId()));
    }

    @Test
    void flightUpdatedEvent_UpdatesFlight() {
        Flight flight = flightRepository.save(createFlight("2023-10-01T10:00:00", 2, 1));

        FlightDto dto = FlightDto.builder()
                .id(flight.getId())
                .departureTime(LocalDateTime.parse("2023-10-01T10:00:00"))
                .arrivalTime(LocalDateTime.parse("2023-10-01T14:00:00"))
                .build();

        FlightChangeEvent event = new FlightChangeEvent(ChangeType.UPDATED, dto);
        jmsTemplate.convertAndSend("flight-changes-for-steward-service", event);

        await().atMost(2, SECONDS).untilAsserted(() -> {
            Flight updated = flightRepository.findById(flight.getId()).orElseThrow();
            assertThat(updated.getDepartureTime()).isEqualToIgnoringNanos(dto.getDepartureTime());
            assertThat(updated.getArrivalTime()).isEqualToIgnoringNanos(dto.getArrivalTime());
        });
    }

    @Test
    void addStewardToNonexistentFlight_ReturnsNotFound() throws Exception {
        Steward steward = stewardRepository.save(Steward.named("Ghost", "Steward"));

        mockMvc.perform(put("/flights/9999/stewards/" + steward.getId()))
                .andExpect(status().isNotFound());
    }

    @Test
    void getStewardsForFlight_WithNoStewards_ReturnsEmptyList() throws Exception {
        Flight flight = flightRepository.save(createFlight("2023-10-01T10:00:00", 2, 1));

        mockMvc.perform(get("/flights/" + flight.getId() + "/stewards"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$", hasSize(0)));
    }

    @Test
    void addStewardTwiceToFlight_DoesNotDuplicate() throws Exception {
        Steward steward = stewardRepository.save(Steward.named("Repeat", "Test"));
        Flight flight = flightRepository.save(createFlight("2023-10-01T10:00:00", 2, 1));

        mockMvc.perform(put("/flights/" + flight.getId() + "/stewards/" + steward.getId()))
                .andExpect(status().isNoContent());

        mockMvc.perform(put("/flights/" + flight.getId() + "/stewards/" + steward.getId()))
                .andExpect(status().isNoContent());

        assertThat(flightRepository.findById(flight.getId()).get().getStewards())
                .containsExactly(steward);
    }
}
