package app.service;

import app.entity.Flight;
import app.exceptions.FlightServiceException;
import app.mapper.FlightEventMapper;
import app.repository.FlightRepository;
import app.struct.Status;
import app.web.FlightChangeProducer;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.domain.Specification;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class FlightService {

    private static final Logger logger = Logger.getLogger(FlightService.class.getName());

    private final FlightRepository flightRepository;

    private final FlightChangeProducer flightChangeProducer;

    private final FlightEventMapper flightEventMapper;

    public FlightService(FlightEventMapper flightEventMapper, FlightChangeProducer flightChangeProducer, FlightRepository flightRepository) {
        this.flightEventMapper = flightEventMapper;
        this.flightChangeProducer = flightChangeProducer;
        this.flightRepository = flightRepository;
    }

    public Optional<Flight> getFlightById(Long id) {
        logger.info("Fetching flight by ID: " + id);
        return flightRepository.findById(id);
    }

    @Transactional
    public Flight createFlight(Flight flight) {
        logger.info("Creating new flight with details: " + flight);
        if (flight.getDepartureAirportId() == null || flight.getArrivalAirportId() == null) {
            logger.warning("Flight must have both departure and arrival airports");
            throw new FlightServiceException("Flight must have both departure and arrival airports", 400);
        }
        if (flight.getDepartureTime().isAfter(flight.getArrivalTime())) {
            logger.warning("Flight departure time must be before arrival time");
            throw new FlightServiceException("Flight departure time must be before arrival time", 400);
        }
        if (flightRepository.existsOverlappingFlight(flight.getPlaneId(), flight.getDepartureTime(),flight.getArrivalTime()))
        {
            logger.warning("Flight overlaps with another flight");
            throw new FlightServiceException("Flight overlaps with another flight", 400);
        }
        if (flightRepository.exists(Specification.where((root, query, criteriaBuilder) ->
                criteriaBuilder.equal(root.get("flightCode"), flight.getFlightCode())))) {
            logger.warning("Flight with code " + flight.getFlightCode() + " already exists");
            throw new FlightServiceException("Flight with code " + flight.getFlightCode() + " already exists", 400);
        }
        var createdFlight = flightRepository.save(flight);
        logger.info("Flight created with ID: " + flight.getId());
        // Send flight change event
        logger.info("Sending flight change event for created flight with ID: " + flight.getId());
        flightChangeProducer.sendFlightChange(FlightChangeEvent.createEvent(flightEventMapper.toEventDto(createdFlight)));
        return createdFlight;
    }

    @Transactional
    public void deleteFlight(Long id) {
        logger.info("Deleting flight with ID: " + id);
        var flight = flightRepository.findById(id);
        if (flight.isEmpty()) {
            logger.warning("Flight with ID " + id + " not found");
            return;
        }
        // Send flight change event
        logger.info("Sending flight change event for deleted flight with ID: " + id);
        flightChangeProducer.sendFlightChange(FlightChangeEvent.deleteEvent(id));
        flightRepository.deleteById(id);
    }

    @Transactional
    public Flight updateFlight(Flight flight) {
        logger.info("Updating flight with ID: " + flight.getId());
        if (flight.getPlaneId() != null)
        {
            var new_departure = flight.getDepartureTime();
            var new_arrival = flight.getArrivalTime();
            if (flightRepository.existsOverlappingFlightExceptCurrentOne(flight.getPlaneId(), new_departure, new_arrival,flight.getId()))
            {
                logger.warning("Flight overlaps with another flight");
                throw new FlightServiceException("Flight overlaps with another flight", 400);
            }
        }
        var flightUpdated = flightRepository.save(flight);
        logger.info("Flight updated with ID: " + flightUpdated.getId());
        // Send flight change event
        logger.info("Sending flight change event for updated flight with ID: " + flightUpdated.getId());
        flightChangeProducer.sendFlightChange(FlightChangeEvent.updateEvent(flightEventMapper.toEventDto(flightUpdated)));
        return flightUpdated;
    }

    public Optional<Flight> getFlightByCode(String code) {
        logger.info("Fetching flight by code: " + code);
        return flightRepository.findByFlightCode(code);
    }

    public List<Flight> getCurrentFlights(Pageable pageable) {
        logger.info("Fetching current flights");
        return flightRepository.getCurrentFlights(null, pageable).getContent();
    }

    @Transactional
    public Optional<Flight> addSteward(Long flightId, Long stewardId) {
        logger.info("Adding steward with ID: " + stewardId + " to flight with id: " + flightId);
        if (flightRepository.findById(flightId).isEmpty()) {
            logger.warning("Flight with id " + flightId + " not found");
            return Optional.empty();
        }
        if (flightRepository.findById(flightId).get().getStewardsIds().contains(stewardId)) {
            logger.warning("Steward with ID " + stewardId + " already assigned to flight " + flightId);
            return Optional.empty();
        }
        flightRepository.addSteward(flightId, stewardId);
        return flightRepository.findById(flightId);
    }

    public List<Flight> getFlightsByStatus(String status, Pageable pageable) {
        logger.info("Fetching flights with status: " + status + ", pageable: " + pageable);
        return flightRepository.findByStatus(Status.valueOf(status), pageable).getContent();
    }


    public List<Flight> getAllFlights(Pageable pageable) {
        logger.info("Fetching all flights with pageable: " + pageable);
        return flightRepository.findAll(pageable).getContent();
    }


    public List<Flight> getFlightsByAvailableSeats(Integer availableSeats, Pageable pageable) {
        logger.info("Fetching flights with available seats > " + availableSeats + ", pageable: " + pageable);
        return flightRepository.findByAvailableSeatsGreaterThan(availableSeats, pageable).getContent();
    }


    @Transactional
    public void deleteFlightsByAirport(Long airportId) {
        flightRepository.deleteByDepartureAirportIdOrArrivalAirportId(airportId, airportId);
    }
    @Transactional
    public void deleteFlightsByAirplaneId(Long airplaneId) {
        flightRepository.deleteByPlaneId(airplaneId);
    }
    @Transactional
    public void unassignSteward(Long flightId, Long stewardCode) {
        logger.info("Unassigning steward with ID: " + stewardCode + " from flight with code: " + flightId);
        var flight = flightRepository.findById(flightId);
        if (flight.isEmpty()) {
            logger.warning("Flight with code " + flightId + " not found");
            return;
        }
        if (!flight.get().getStewardsIds().contains(stewardCode)) {
            logger.warning("Steward with ID " + stewardCode + " not assigned to flight " + flightId);
            return;
        }
        flight.get().getStewardsIds().remove(stewardCode);
        flightRepository.save(flight.get());
    }
    public List<Flight> getFlightsWithSteward(Long stewardId) {
        return flightRepository.findFlightsByStewardId(stewardId);
    }

}
