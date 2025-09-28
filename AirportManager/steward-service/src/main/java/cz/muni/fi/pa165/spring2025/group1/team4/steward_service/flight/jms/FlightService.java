package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.HashSet;

@Service
@RequiredArgsConstructor
public class FlightService {

    private final FlightRepository flightRepository;

    @Transactional
    public void create(Flight flight) {
        flightRepository.save(flight);
    }

    @Transactional
    public void update(Flight flight) {
        flightRepository.save(flight);
    }

    @Transactional
    public void deleteById(Long id) {
        flightRepository.findById(id).ifPresent(flight -> {
            flight.setStewards(new HashSet<>());
            flightRepository.save(flight);
            flightRepository.deleteById(id);
        });
    }

}
