package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Optional;

@Service
@RequiredArgsConstructor
public class StewardFacade {

    private final StewardService stewardService;
    private final StewardMapper stewardMapper;

    @Transactional(readOnly = true)
    public Optional<StewardDto> findById(Long id) {
        return stewardService.findById(id).map(steward -> stewardMapper.toDto(steward));
    }

    @Transactional(readOnly = true)
    public Page<StewardDto> findAllStewards(Pageable pageable) {
        return stewardMapper.toPage(stewardService.findAll(pageable));
    }

    @Transactional
    public StewardDto createSteward(StewardNewDto stewardDto) {
        Steward steward = stewardMapper.toSteward(stewardDto);
        Steward persistedSteward = stewardService.createSteward(steward);
        return stewardMapper.toDto(persistedSteward);
    }

    public StewardDto updateSteward(StewardDto stewardNewDto) throws ResourceNotFoundException {
        Steward steward = stewardMapper.toSteward(stewardNewDto);
        Steward persistedSteward = stewardService.updateSteward(steward);
        return stewardMapper.toDto(persistedSteward);
    }

    public void deleteSteward(Long stewardId) {
        stewardService.deleteSteward(Steward.withId(stewardId));
    }

}
