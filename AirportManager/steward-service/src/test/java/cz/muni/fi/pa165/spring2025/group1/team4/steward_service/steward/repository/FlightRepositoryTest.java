package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.repository;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.boot.test.autoconfigure.orm.jpa.TestEntityManager;

import java.time.LocalDateTime;
import java.util.List;
import java.util.function.Supplier;

import static org.assertj.core.api.Assertions.assertThat;

@DataJpaTest
class FlightRepositoryTest {

    @Autowired
    private TestEntityManager em;

    @Autowired
    private FlightRepository flights;

    private static Flight flightSpanning(String from, String to, Long id) {
        Flight flight = Flight.spanning(LocalDateTime.parse(from), LocalDateTime.parse(to));
        flight.setId(id);
        return flight;
    }

    private static final Supplier<Flight> middleFlight = () -> flightSpanning(
            "2004-03-11T10:03:00", "2004-03-12T03:41:00", 1L);

    private static final Supplier<Flight> nonOverlappingFlightAfter = () -> flightSpanning(
            "2004-04-01T04:45:00", "2004-04-01T12:35:00", 2L);

    private static final Supplier<Flight> nonOverlappingFlightBefore = () -> flightSpanning(
            "2004-03-10T20:29:00", "2004-03-11T05:25:00", 3L);

    private static final Supplier<Flight> overlappingBeforeMiddleFlight = () -> flightSpanning(
            "2004-03-11T06:59:00", "2004-03-11T16:33:00", 4L);

    private static final Supplier<Flight> overlappingAfterMiddleFlight = () -> flightSpanning(
            "2004-03-12T01:33:00", "2004-03-12T07:08:00", 5L);

    private static final Supplier<Flight> overlappingAroundMiddleFlight = () -> flightSpanning(
            "2004-03-11T09:03:00", "2004-03-12T04:41:00", 6L);

    @Test
    void count_ReturnsOne_AfterFlightInsterted() {
        em.persist(middleFlight.get());
        em.flush();

        assertThat(flights.count()).as("Number of flights").isEqualTo(1);
    }

    @Test
    void existsConflict_ReturnsFalse_WhenNoConflictingFlight() {
        Steward steward = Steward.named("Goober", "McDoober");
        Flight busyFlightOne = nonOverlappingFlightBefore.get();
        Flight desiredFlight = middleFlight.get();
        Flight busyFlightTwo = nonOverlappingFlightAfter.get();

        busyFlightOne.getStewards().add(steward);
        busyFlightTwo.getStewards().add(steward);

        em.persist(steward);
        em.persist(busyFlightOne);
        em.persist(busyFlightTwo);
        em.flush();

        boolean isConflict = flights.existsFlightWithStewardBetweenGivenTimes(steward,
                desiredFlight.getDepartureTime(),
                desiredFlight.getArrivalTime());

        assertThat(isConflict).as("Conflict present").isFalse();
    }

    @Test
    void existsConflict_ReturnsFalse_WhenConflictingFlightsStaffedBySomeoneElse() {
        Steward steward = Steward.named("Steward", "One");
        Steward stewardTwo = Steward.named("Steward", "Two");
        Steward stewardThree = Steward.named("Steward", "Three");
        Flight desiredFlight = middleFlight.get();
        Flight busyFlightBefore = overlappingBeforeMiddleFlight.get();
        Flight busyFlightAfter = overlappingAfterMiddleFlight.get();
        Flight busyFlightAround = overlappingAroundMiddleFlight.get();

        busyFlightBefore.getStewards().add(stewardTwo);
        busyFlightAfter.getStewards().add(stewardTwo);
        busyFlightAround.getStewards().add(stewardThree);

        for (var entity : List.of(steward, stewardTwo, stewardThree,
                desiredFlight, busyFlightBefore, busyFlightAfter, busyFlightAfter, busyFlightAround)) {
            em.persist(entity);
        }
        em.flush();

        boolean isConflict = flights.existsFlightWithStewardBetweenGivenTimes(steward,
                desiredFlight.getDepartureTime(),
                desiredFlight.getArrivalTime());

        assertThat(isConflict).as("Conflict present").isFalse();
    }

    @Test
    void existsConflict_ReturnsTrue_WhenStewardIsOnOverlappingFlightBefore() {
        Steward steward = Steward.named("Benjamin", "McGee");
        Flight desiredFlight = middleFlight.get();
        Flight busyFlightBefore = overlappingBeforeMiddleFlight.get();

        busyFlightBefore.getStewards().add(steward);

        em.persist(steward);
        em.persist(busyFlightBefore);
        em.flush();

        boolean isConflict = flights.existsFlightWithStewardBetweenGivenTimes(steward,
                desiredFlight.getDepartureTime(),
                desiredFlight.getArrivalTime());

        assertThat(isConflict).as("Conflict present").isTrue();
    }

    @Test
    void existsConflict_ReturnsTrue_WhenStewardIsOnOverlappingFlightAfter() {
        Steward steward = Steward.named("Benjamin", "McGee");
        Flight desiredFlight = middleFlight.get();
        Flight busyFlightAfter = overlappingAfterMiddleFlight.get();

        busyFlightAfter.getStewards().add(steward);

        em.persist(steward);
        em.persist(busyFlightAfter);
        em.flush();

        boolean isConflict = flights.existsFlightWithStewardBetweenGivenTimes(steward,
                desiredFlight.getDepartureTime(),
                desiredFlight.getArrivalTime());

        assertThat(isConflict).as("Conflict present").isTrue();
    }

    @Test
    void existsConflict_ReturnsTrue_WhenStewardIsOnOverlappingFlightAround() {
        Steward steward = Steward.named("Benjamin", "McGee");
        Flight desiredFlight = middleFlight.get();
        Flight busyFlightAround = overlappingAroundMiddleFlight.get();

        busyFlightAround.getStewards().add(steward);

        em.persist(steward);
        em.persist(busyFlightAround);
        em.flush();

        boolean isConflict = flights.existsFlightWithStewardBetweenGivenTimes(steward,
                desiredFlight.getDepartureTime(),
                desiredFlight.getArrivalTime());

        assertThat(isConflict).as("Conflict present").isTrue();
    }
}
