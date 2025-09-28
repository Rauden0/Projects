package app.repository;

import app.airport.entity.Airport;
import app.airport.repository.AirportRepository;
import jakarta.transaction.Transactional;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.test.context.junit.jupiter.SpringExtension;

import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

@ExtendWith(SpringExtension.class)
@DataJpaTest
public class AirportRepositoryTest {

    @Autowired
    private AirportRepository repository;

    @Test
    @Transactional
    public void testSaveAndFindById() {
        Airport airport = new Airport();
        airport.setName("Heathrow");
        airport.setCity("London");
        airport.setCountry("UK");

        Airport savedAirport = repository.save(airport);
        Optional<Airport> foundAirport = repository.findById(savedAirport.getId());

        assertTrue(foundAirport.isPresent());
        assertEquals("Heathrow", foundAirport.get().getName());
    }

    @Test
    public void testFindAll() {
        Airport airport1 = new Airport();
        airport1.setName("Heathrow");
        airport1.setCity("London");
        airport1.setCountry("UK");

        Airport airport2 = new Airport();
        airport2.setName("JFK");
        airport2.setCity("New York");
        airport2.setCountry("USA");

        repository.save(airport1);
        repository.save(airport2);

        List<Airport> airports = repository.findAll();
        assertEquals(2, airports.size());
    }

    @Test
    @Transactional
    public void testDeleteById() {
        Airport airport = new Airport();
        airport.setName("Heathrow");
        airport.setCity("London");
        airport.setCountry("UK");

        Airport savedAirport = repository.save(airport);
        repository.deleteById(savedAirport.getId());

        Optional<Airport> foundAirport = repository.findById(savedAirport.getId());
        assertFalse(foundAirport.isPresent());
    }

    @Test
    @Transactional
    public void testUpdateAirport() {
        Airport airport = new Airport();
        airport.setName("Heathrow");
        airport.setCity("London");
        airport.setCountry("UK");

        repository.save(airport);

        airport.setName("Gatwick");
        Airport updatedAirport = repository.save(airport);

        Optional<Airport> foundAirport = repository.findById(updatedAirport.getId());
        assertTrue(foundAirport.isPresent());
        assertEquals("Gatwick", foundAirport.get().getName());
    }

}