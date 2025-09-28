package app.airport.service;

import app.airport.entity.Airport;
import app.airport.mapper.AirportEventMapper;
import app.airport.repository.AirportRepository;
import app.airport.web.AirportChangeProducer;
import app.exceptions.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class AirportService {
    private static final Logger logger = Logger.getLogger(AirportService.class.getName());

    private final AirportRepository repository;

    private final AirportChangeProducer changeProducer;

    private final AirportEventMapper eventMapper;

    public AirportService(AirportEventMapper eventMapper, AirportChangeProducer changeProducer, AirportRepository repository) {
        this.eventMapper = eventMapper;
        this.changeProducer = changeProducer;
        this.repository = repository;
    }

    public List<Airport> getAllAirports() {
        logger.info("Getting all Airports");
        return repository.findAll();
    }

    public Optional<Airport> getAirportById(Long id) {
        logger.info("Getting Airport by ID " + id);
        return repository.findById(id);
    }

    @Transactional
    public Airport createAirport(Airport airport) {
        logger.info("Creating Airport: " + airport);

        var newAirport = repository.save(airport);
        logger.info("Airport created with ID: " + airport.getId());
        logger.info("Sending airport change event for created airport with ID: " + airport.getId());
        changeProducer.sendAirportChange(AirportChangeEvent.createEvent(eventMapper.toEventDto(newAirport)));

        return newAirport;
    }

    @Transactional
    public void deleteAirport(Long id) {
        logger.info("Deleting Airport: " + id);

        if (!repository.existsById(id)) {
            throw new ResourceNotFoundException("Airport not found with id: " + id);
        }

        logger.info("Sending airport change event for deleted airport with ID: " + id);
        changeProducer.sendAirportChange(AirportChangeEvent.deleteEvent(id));

        repository.deleteById(id);
    }

    @Transactional
    public Airport updateAirport(Airport airport) {
        logger.info("Updating Airport: " + airport);
        if (!repository.existsById(airport.getId())) {
            throw new ResourceNotFoundException("Airport not found with id: " + airport.getId());
        }

        if (airport.getId() != null)
        {
            var newCountry = airport.getCountry();
            var newCity = airport.getCity();
            var newName = airport.getName();
            if (repository.existsOverlappingAirportExceptCurrentOne(airport.getId(), newCountry, newCity, newName))
            {
                logger.warning("Airport overlaps with another airport");
                throw new ResourceNotFoundException("Airport overlaps with another airport");
            }
        }

        var updatedAirplane = repository.save(airport);
        logger.info("Sending airport change event for updated airport with ID: " + updatedAirplane.getId());
        changeProducer.sendAirportChange(AirportChangeEvent.updateEvent(eventMapper.toEventDto(updatedAirplane)));

        return updatedAirplane;
    }

    public boolean existsById(Long id) {
        return repository.existsById(id);
    }
}
