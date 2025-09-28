package app.airport.facade;

import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.mapper.AirportMapper;
import app.airport.service.AirportService;
import app.exceptions.ResourceNotFoundException;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class AirportFacade {
    private static final Logger logger = Logger.getLogger(AirportFacade.class.getName());

    private final AirportService service;
    private final AirportMapper mapper;

    public AirportFacade(AirportService service, AirportMapper mapper) {
        this.service = service;
        this.mapper = mapper;
    }

    public List<AirportDTO> getAllAirports() {
        logger.info("Getting all Airports");
        return mapper.toDTOList(service.getAllAirports());
    }

    public Optional<AirportDTO> getAirportById(Long id) {
        logger.info("Getting airport by ID: " + id);
        return service.getAirportById(id).map(mapper::toDTO);
    }

    public void deleteAirport(Long id) {
        if (!service.existsById(id)) {
            throw new ResourceNotFoundException("Airport not found with id: " + id);
        }
        service.deleteAirport(id);
    }

    public AirportDTO updateAirport(AirportUpdateDTO airport) {
        var existingAirport = service.getAirportById(airport.getId());
        if (existingAirport.isEmpty()) {
            logger.warning("Airport with ID " + airport.getId() + " not found");
            throw new ResourceNotFoundException("Airport not found with id: " + airport.getId());
        }
        mapper.updateAirportFromDto(airport, existingAirport.get());

        return mapper.toDTO(service.updateAirport(existingAirport.get()));
    }

    public AirportDTO createAirport(AirportCreateDTO airport) {
        logger.info("Creating Airport: " + airport);
        return mapper.toDTO(service.createAirport(mapper.toEntity(airport)));
    }
}
