package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.repository;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.boot.test.autoconfigure.orm.jpa.TestEntityManager;

import java.time.LocalDateTime;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

@DataJpaTest
class FlightRepositoryTest {

    @Autowired
    private TestEntityManager em;

    @Autowired
    private FlightRepository repository;

    @Test
    void findByIdReturnsFlightWhenExists() {
        Flight flight = new Flight();
        flight.setId(1L);
        flight.setArrivalTime(LocalDateTime.parse("2023-10-01T10:00:00"));
        flight = em.persist(flight);
        em.flush();

        Optional<Flight> found = repository.findById(flight.getId());

        assertTrue(found.isPresent());
        assertEquals(LocalDateTime.parse("2023-10-01T10:00:00"), found.get().getArrivalTime());
    }

    @Test
    void findByIdReturnsEmptyWhenNotExists() {
        Optional<Flight> result = repository.findById(3L);
        assertTrue(result.isEmpty());
    }

    @Test
    void savePersistsNewFlight() {
        Flight flight = new Flight();
        flight.setId(1L);
        flight.setArrivalTime(LocalDateTime.parse("2023-10-01T10:00:00"));
        Flight saved = repository.save(flight);

        assertNotNull(saved.getId());
        Flight persisted = em.find(Flight.class, saved.getId());
        assertEquals(LocalDateTime.parse("2023-10-01T10:00:00"), persisted.getArrivalTime());
    }
}
