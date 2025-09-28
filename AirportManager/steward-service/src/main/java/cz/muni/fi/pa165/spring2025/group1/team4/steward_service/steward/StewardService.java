package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms.StewardEventDispatcher;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Optional;

@Service
@RequiredArgsConstructor
public class StewardService {
    private final StewardRepository stewardRepository;

    private final StewardEventDispatcher eventDispatcher;

    @Transactional(readOnly = true)
    public Optional<Steward> findById(Long id) {
        return stewardRepository.findById(id);
    }

    @Transactional(readOnly = true)
    public Page<Steward> findAll(Pageable pageable) {
        return stewardRepository.findAll(pageable);
    }

    @Transactional
    public Steward createSteward(Steward steward) {
        Steward managedSteward = stewardRepository.save(steward);
        eventDispatcher.emitCreation(managedSteward);
        return managedSteward;
    }

    @Transactional
    public Steward updateSteward(Steward stewardUpdates) throws ResourceNotFoundException {
        stewardRepository.find(Steward.withId(stewardUpdates.getId()));
        Steward steward = stewardRepository.save(stewardUpdates);
        eventDispatcher.emitUpdate(stewardUpdates);
        return steward;
    }

    @Transactional
    public void deleteSteward(Steward steward) {
        stewardRepository.delete(steward);
        eventDispatcher.emitDelete(steward.getId());
    }
}
