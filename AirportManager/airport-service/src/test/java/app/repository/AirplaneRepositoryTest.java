package app.repository;

import app.airplane.entity.Airplane;
import app.airplane.entity.AirplaneType;
import app.airplane.repository.AirplaneRepository;
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
public class AirplaneRepositoryTest {

    @Autowired
    private AirplaneRepository repository;

    @Test
    @Transactional
    public void testSaveAndFindById() {
        Airplane airplane = new Airplane();
        airplane.setName("Boeing 747");
        airplane.setType(AirplaneType.COMMERCIAL);
        airplane.setCapacity(300);
        Airplane savedAirplane = repository.save(airplane);
        Optional<Airplane> foundAirplane = repository.findById(savedAirplane.getId());

        assertTrue(foundAirplane.isPresent());
        assertEquals("Boeing 747", foundAirplane.get().getName());
    }

    @Test
    public void testFindAll() {
        Airplane airplane1 = new Airplane();
        airplane1.setName("Boeing 747");
        airplane1.setType(AirplaneType.COMMERCIAL);
        airplane1.setCapacity(300);

        Airplane airplane2 = new Airplane();
        airplane2.setName("Airbus A380");
        airplane2.setType(AirplaneType.COMMERCIAL);
        airplane2.setCapacity(500);

        repository.save(airplane1);
        repository.save(airplane2);

        List<Airplane> airplanes = repository.findAll();
        assertEquals(2, airplanes.size());
    }

    @Test
    @Transactional
    public void testDeleteById() {
        Airplane airplane = new Airplane();
        airplane.setName("Boeing 747");
        airplane.setType(AirplaneType.COMMERCIAL);
        airplane.setCapacity(300);

        Airplane savedAirplane = repository.save(airplane);
        repository.deleteById(savedAirplane.getId());

        Optional<Airplane> foundAirplane = repository.findById(savedAirplane.getId());
        assertFalse(foundAirplane.isPresent());
    }

    @Test
    @Transactional
    public void testUpdateAirplane() {
        Airplane airplane = new Airplane();
        airplane.setName("Boeing 747");
        airplane.setType(AirplaneType.COMMERCIAL);
        airplane.setCapacity(300);

        repository.save(airplane);

        airplane.setName("Boeing 777");
        Airplane updatedAirplane = repository.save(airplane);

        Optional<Airplane> foundAirplane = repository.findById(updatedAirplane.getId());
        assertTrue(foundAirplane.isPresent());
        assertEquals("Boeing 777", foundAirplane.get().getName());
    }
}