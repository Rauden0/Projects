package app.controller;

import app.config.SecurityConfiguration;
import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.facade.FlightFacade;
import io.swagger.annotations.ApiParam;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import jakarta.validation.Valid;
import org.springframework.data.domain.Pageable;
import org.springframework.data.web.PageableDefault;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.logging.Logger;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.*;

@RestController
@RequestMapping("/flights")
public class FlightController {

    private static final Logger logger = Logger.getLogger(FlightController.class.getName());
    private final FlightFacade flightFacade;

    public FlightController(FlightFacade flightFacade) {
        this.flightFacade = flightFacade;
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT,
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @GetMapping
    public List<FlightDTO> getFlights(@PageableDefault() Pageable pageable) {
        logger.info("Fetching flights with pagination - page: " + pageable.getPageNumber() + ", size: " + pageable.getPageSize());
        return flightFacade.getAllFlights(pageable);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        COMMERCIAL_DEPT,
        SCHEDULE_COORDINATOR,
        HR_AND_MANAGEMENT
    }))
    @GetMapping(value = "/code/{code}", produces = "application/json")
    public ResponseEntity<FlightDTO> getFlightByCode(
            @ApiParam(value = "The flight code", required = true) @PathVariable("code") String code) {
        logger.info("Fetching flight by code: " + code);
        return ResponseEntity.of(flightFacade.getFlightByCode(code));
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        COMMERCIAL_DEPT,
        SCHEDULE_COORDINATOR,
        HR_AND_MANAGEMENT
    }))
    @GetMapping("/current")
    public List<FlightDTO> getCurrentFlights(@PageableDefault() Pageable pageable) {
        logger.info("Fetching current flights");
        return flightFacade.getCurrentFlights(pageable);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        COMMERCIAL_DEPT,
        SCHEDULE_COORDINATOR,
        HR_AND_MANAGEMENT
    }))
    @GetMapping(value = "{id}", produces = "application/json")
    public ResponseEntity<FlightDTO> getFlightById(
            @ApiParam(value = "The flight id", required = true) @PathVariable("id") Long id) {
        logger.info("Fetching flight by ID: " + id);
        return ResponseEntity.of(flightFacade.getFlightById(id));
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        SCHEDULE_COORDINATOR
    }))
    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public FlightDTO createFlight(@RequestBody @Valid FlightCreateDTO flight) {
        logger.info("Creating new flight with details: " + flight);
        return flightFacade.createFlight(flight);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        SCHEDULE_COORDINATOR
    }))
    @DeleteMapping(value = { "/{id}" })
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void deleteFlight(
            @ApiParam(value = "The flight id", required = true) @PathVariable("id") Long id) {
        logger.info("Deleting flight with ID: " + id);
        flightFacade.deleteFlight(id);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        SCHEDULE_COORDINATOR
    }))
    @PutMapping
    public FlightDTO updateFlight(@Valid @RequestBody FlightUpdateDTO flight) {
        logger.info("Updating flight with ID: " + flight.getId());
        return flightFacade.updateFlight(flight);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        COMMERCIAL_DEPT,
        SCHEDULE_COORDINATOR,
        HR_AND_MANAGEMENT
    }))
    @GetMapping("/byStatus")
    public List<FlightDTO> getFlightsByStatus(
            @ApiParam(value = "The flight status", required = true) @RequestParam("status") String status,
            @PageableDefault() Pageable pageable) {
        logger.info("Fetching flights with status: " + status + " page: " + pageable.getPageNumber() + ", size: " + pageable.getPageSize());
        return flightFacade.getFlightsByStatus(status, pageable);
    }

    @Operation(security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
        COMMERCIAL_DEPT,
        SCHEDULE_COORDINATOR,
        HR_AND_MANAGEMENT
    }))
    @GetMapping("/byAvailableSeats")
    public List<FlightDTO> getFlightsByAvailableSeats(
            @ApiParam(value = "The minimum number of available seats", required = true) @RequestParam("availableSeats") Integer availableSeats,
            @PageableDefault() Pageable pageable) {
        logger.info("Fetching flights with available seats > " + availableSeats + " page: " + pageable.getPageNumber() + ", size: " + pageable.getPageSize());
        return flightFacade.getFlightsByAvailableSeats(availableSeats, pageable);
    }

}
