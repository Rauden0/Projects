package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.repository;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardRepository;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.boot.test.autoconfigure.orm.jpa.TestEntityManager;

import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

@DataJpaTest
class StewardRepositoryTest {

    @Autowired
    private TestEntityManager em;

    @Autowired
    private StewardRepository repository;

    @Test
    void findByIdReturnsStewardWhenExists() {
        Steward steward = Steward.named("Tonda", "Tondovicz");
        em.persist(steward);
        em.flush();

        Optional<Steward> found = repository.findById(steward.getId());
        assertTrue(found.isPresent());
        assertEquals("Tonda", found.get().getGivenName());
    }

    @Test
    void findByIdReturnsEmptyWhenNotExists() {
        Optional<Steward> found = repository.findById(999L);
        assertTrue(found.isEmpty());
    }

    @Test
    void savePersistsNewSteward() {
        Steward steward = Steward.named("Tonda", "Tondovicz");
        Steward saved = repository.save(steward);

        assertNotNull(saved.getId());
        assertEquals("Tonda", em.find(Steward.class, saved.getId()).getGivenName());
    }
}