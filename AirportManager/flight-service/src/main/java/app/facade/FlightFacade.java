package app.facade;

import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.exceptions.AirplaneNotFoundException;
import app.exceptions.AirportNotFoundException;
import app.exceptions.FlightNotFoundException;
import app.mapper.FlightMapper;
import app.service.AirplaneService;
import app.service.AirportService;
import app.service.FlightService;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class FlightFacade {

    private static final Logger logger = Logger.getLogger(FlightFacade.class.getName());
    private final FlightService flightService;
    private final FlightMapper flightMapper;
    private final AirportService airportService;
    private final AirplaneService airplaneService;

    public FlightFacade(FlightService flightService, AirplaneService airplaneService, AirportService airportService, FlightMapper flightMapper) {
        this.flightService = flightService;
        this.flightMapper = flightMapper;
        this.airportService = airportService;
        this.airplaneService = airplaneService;
    }

    public Optional<FlightDTO> getFlightById(Long id) {
        logger.info("Fetching flight by ID: " + id);
        return flightService.getFlightById(id).map(flightMapper::toDTO);
    }

    public void deleteFlight(Long id) {
        logger.info("Deleting flight with ID: " + id);
        flightService.deleteFlight(id);
    }

    public FlightDTO updateFlight(FlightUpdateDTO flightUpdateDTO) {
        logger.info("Updating flight with ID: " + flightUpdateDTO.getId());

        if (flightUpdateDTO.getDepartureAirportId() != null) {
            logger.info("Validating arrival airport: " + flightUpdateDTO.getArrivalAirportId());
            if (airportService.getAirportById(flightUpdateDTO.getArrivalAirportId()) == null) {
                throw new AirportNotFoundException("Airport with ID " + flightUpdateDTO.getArrivalAirportId() + " not found");
            }
        }
        if (flightUpdateDTO.getDepartureAirportId() != null) {
            logger.info("Validating departure airport: " + flightUpdateDTO.getDepartureAirportId());
            if (airportService.getAirportById(flightUpdateDTO.getDepartureAirportId()) == null) {
                throw new AirportNotFoundException("Airport with ID " + flightUpdateDTO.getDepartureAirportId() + " not found");
            }
        }
        if (flightUpdateDTO.getPlaneId() != null) {
            logger.info("Validating plane: " + flightUpdateDTO.getPlaneId());
            if (airplaneService.getAirplaneById(flightUpdateDTO.getPlaneId()) == null) {
                throw new AirplaneNotFoundException("Airplane with ID " + flightUpdateDTO.getPlaneId() + " not found");
            }
        }
        var existingFlight = flightService.getFlightById(flightUpdateDTO.getId());
        if (existingFlight.isEmpty()) {
            logger.warning("Flight with ID " + flightUpdateDTO.getId() + " not found");
            throw new FlightNotFoundException("Flight with ID " + flightUpdateDTO.getId() + " not found");
        }
        flightMapper.updateFlightFromDto(flightUpdateDTO, existingFlight.get());

        return flightMapper.toDTO(flightService.updateFlight(existingFlight.get()));
    }

    public FlightDTO createFlight(FlightCreateDTO flight) {
        logger.info("Creating new flight with details: " + flight);

        logger.info("Validating arrival airport: " + flight.getArrivalAirportId());
        if (airportService.getAirportById(flight.getArrivalAirportId()) == null) {
            throw new AirportNotFoundException("Airport with ID " + flight.getArrivalAirportId() + " not found");
        }

        logger.info("Validating departure airport: " + flight.getDepartureAirportId());
        if (airportService.getAirportById(flight.getDepartureAirportId()) == null) {
            throw new AirportNotFoundException("Airport with ID " + flight.getDepartureAirportId() + " not found");
        }

        logger.info("Validating plane: " + flight.getPlaneId());
        if (airplaneService.getAirplaneById(flight.getPlaneId()) == null) {
            throw new AirplaneNotFoundException("Airplane with ID " + flight.getPlaneId() + " not found");
        }

        return flightMapper.toDTO(flightService.createFlight(flightMapper.toEntity(flight)));
    }

    public Optional<FlightDTO> getFlightByCode(String code) {
        logger.info("Fetching flight by code: " + code);
        return flightService.getFlightByCode(code).map(flightMapper::toDTO);
    }

    public List<FlightDTO> getCurrentFlights(Pageable pageable) {
        logger.info("Fetching current flights");
        return flightMapper.toDTOList(flightService.getCurrentFlights(pageable));
    }

    public List<FlightDTO> getFlightsByStatus(String status, Pageable pageable) {
        logger.info("Fetching flights with status: " + status);
        return flightMapper.toDTOList(flightService.getFlightsByStatus(status, pageable));
    }

    public List<FlightDTO> getFlightsByAvailableSeats(Integer availableSeats, Pageable pageable) {
        logger.info("Fetching flights with available seats > " + availableSeats);
        return flightMapper.toDTOList(flightService.getFlightsByAvailableSeats(availableSeats, pageable));
    }

    public List<FlightDTO> getAllFlights(Pageable pageable) {
        logger.info("Fetching all flights with pagination");
        return flightMapper.toDTOList(flightService.getAllFlights(pageable));
    }

}


