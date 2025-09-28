package app.airplane.service;

import app.airplane.entity.Airplane;
import app.airplane.mapper.AirplaneEventMapper;
import app.airplane.repository.AirplaneRepository;
import app.airplane.web.AirplaneChangeProducer;
import app.exceptions.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class AirplaneService {
    private static final Logger logger = Logger.getLogger(AirplaneService.class.getName());

    private final AirplaneRepository repository;

    private final AirplaneChangeProducer changeProducer;

    private final AirplaneEventMapper eventMapper;

    public AirplaneService(AirplaneEventMapper eventMapper, AirplaneChangeProducer changeProducer, AirplaneRepository repository) {
        this.eventMapper = eventMapper;
        this.changeProducer = changeProducer;
        this.repository = repository;
    }

    public List<Airplane> getAllAirplanes() {
        logger.info("Getting all Airplanes");
        return repository.findAll();
    }

    public Optional<Airplane> getAirplaneById(Long id) {
        logger.info("Getting Airplane by ID " + id);
        return repository.findById(id);
    }

    @Transactional
    public Airplane createAirplane(Airplane airplane) {
        logger.info("Creating Airplane: " + airplane);

        var newAirplane = repository.save(airplane);
        logger.info("Airplane created with ID: " + airplane.getId());
        logger.info("Sending airplane change event for created airplane with ID: " + airplane.getId());
        changeProducer.sendAirplaneChange(AirplaneChangeEvent.createEvent(eventMapper.toEventDto(newAirplane)));

        return newAirplane;
    }

    @Transactional
    public void deleteAirplane(Long id) {
        logger.info("Deleting Airplane: " + id);

        if (!repository.existsById(id)) {
            throw new ResourceNotFoundException("Airplane not found with id: " + id);
        }

        logger.info("Sending airplane change event for deleted airplane with ID: " + id);
        changeProducer.sendAirplaneChange(AirplaneChangeEvent.deleteEvent(id));
        repository.deleteById(id);
    }

    @Transactional
    public Airplane updateAirplane(Airplane airplane) {
        logger.info("Updating Airplane: " + airplane);
        if (!repository.existsById(airplane.getId())) {
            throw new ResourceNotFoundException("Airplane not found with id: " + airplane.getId());
        }

        if (airplane.getId() != null)
        {
            var newType = airplane.getType();
            var newCapacity = airplane.getCapacity();
            var newName = airplane.getName();
            if (repository.existsOverlappingAirplaneExceptCurrentOne(airplane.getId(), newType, newCapacity, newName))
            {
                logger.warning("Airport overlaps with another airplane");
                throw new ResourceNotFoundException("Airport overlaps with another airplane");
            }
        }


        var updatedAirplane = repository.save(airplane);
        logger.info("Sending airplane change event for updated airplane with ID: " + updatedAirplane.getId());
        changeProducer.sendAirplaneChange(AirplaneChangeEvent.updateEvent(eventMapper.toEventDto(updatedAirplane)));

        return updatedAirplane;
    }

    public boolean existsById(Long id) {
        return repository.existsById(id);
    }
}
