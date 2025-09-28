package app.service;

import app.entity.Airport;
import app.repository.AirportRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class AirportService {

    @Autowired
    private AirportRepository airportRepository;

    public Airport getAirportById(Long id) {
        return airportRepository.findById(id).orElse(null);
    }
    @Transactional
    public Airport createAirport(Airport airport) {
        return airportRepository.save(airport);
    }
    @Transactional
    public void deleteAirport(Long id) {
        airportRepository.deleteById(id);
    }
    @Transactional
    public Airport updateAirport(Airport airport) {
        return airportRepository.save(airport);
    }

}
