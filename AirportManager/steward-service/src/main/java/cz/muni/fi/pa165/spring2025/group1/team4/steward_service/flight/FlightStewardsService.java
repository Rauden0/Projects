package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.StewardAssignmentEventDispatcher;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@RequiredArgsConstructor
@Transactional
public class FlightStewardsService {

    private final FlightRepository flightRepository;
    private final StewardAssignmentEventDispatcher dispatcher;

    @Transactional(readOnly = true)
    public List<Steward> findAll(Flight flight) throws ResourceNotFoundException {
        return flightRepository.find(flight).getStewards().stream().toList();
    }

    @Transactional
    public void addFlightSteward(Flight flight, Steward steward) throws ResourceNotFoundException, ResourceConflictException {
        Flight flightManaged = flightRepository.find(flight);
        if (flightManaged.getStewards().contains(steward)) {
            return;
        }
        if (stewardBusyDuringFlightDuration(steward, flightManaged)) {
            throw ResourceConflictException.notAvailable("steward");
        }
        flightManaged.getStewards().add(steward);
        dispatcher.emitCreation(steward, flightManaged);
    }

    private boolean stewardBusyDuringFlightDuration(Steward steward, Flight realFlight) {
        return flightRepository.existsFlightWithStewardBetweenGivenTimes(steward,
                realFlight.getDepartureTime(), realFlight.getArrivalTime());
    }

    @Transactional
    public void removeFlightSteward(Flight flight, Steward steward) throws ResourceNotFoundException {
        flightRepository.find(flight).getStewards().remove(steward);
        dispatcher.emitDelete(steward, flight);
    }

}
