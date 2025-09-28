package app.airplane.facade;

import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.mapper.AirplaneMapper;
import app.airplane.service.AirplaneService;
import app.exceptions.ResourceNotFoundException;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class AirplaneFacade {
    private static final Logger logger = Logger.getLogger(AirplaneFacade.class.getName());

    private final AirplaneService service;
    private final AirplaneMapper mapper;

    public AirplaneFacade(AirplaneService service, AirplaneMapper mapper) {
        this.service = service;
        this.mapper = mapper;
    }

    public List<AirplaneDTO> getAllAirplanes() {
        logger.info("Getting all Airplanes");
        return mapper.toDTOList(service.getAllAirplanes());
    }

    public Optional<AirplaneDTO> getAirplaneById(Long id) {
        logger.info("Getting airplane by ID: " + id);
        return service.getAirplaneById(id).map(mapper::toDTO);
    }

    public void deleteAirplane(Long id) {
        if (!service.existsById(id)) {
            throw new ResourceNotFoundException("Airplane not found with id: " + id);
        }
        service.deleteAirplane(id);
    }

    public AirplaneDTO updateAirplane(AirplaneUpdateDTO airplane) {
        var existingAirplane = service.getAirplaneById(airplane.getId());
        if (existingAirplane.isEmpty()) {
            logger.warning("Airport with ID " + airplane.getId() + " not found");
            throw new ResourceNotFoundException("airplane not found with id: " + airplane.getId());
        }
        mapper.updateAirplaneFromDto(airplane, existingAirplane.get());

        return mapper.toDTO(service.updateAirplane(existingAirplane.get()));
    }

    public AirplaneDTO createAirplane(AirplaneCreateDTO airplane) {
        logger.info("Creating Airplane: " + airplane);
        return mapper.toDTO(service.createAirplane(mapper.toEntity(airplane)));
    }
}
