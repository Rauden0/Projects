package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class FlightStewardsFacade {

    private final FlightStewardsService stewardsService;
    private final FlightStewardsMapper stewardsMapper;

    public List<StewardDto> findAll(Long id) throws ResourceNotFoundException {
        return stewardsMapper.toList(stewardsService.findAll(Flight.withId(id)));
    }

    public void addFlightSteward(Long id, Long stewardId) throws ResourceNotFoundException, ResourceConflictException {
        stewardsService.addFlightSteward(Flight.withId(id), Steward.withId(stewardId));
    }

    public void removeFlightSteward(Long id, Long stewardId) throws ResourceNotFoundException {
        stewardsService.removeFlightSteward(Flight.withId(id), Steward.withId(stewardId));
    }

}
