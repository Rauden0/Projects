package app.airport.controller;

import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.facade.AirportFacade;
import app.security.SecurityConfiguration;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.responses.ApiResponses;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.net.URI;
import java.util.List;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.*;

@RestController
@RequestMapping("/airports")
@Tag(name = "Airports", description = "Endpoints for managing airports")
public class AirportController {
    private final AirportFacade facade;

    public AirportController(AirportFacade facade) {
        this.facade = facade;
    }

    @GetMapping(produces = "application/json")
    @Operation(summary = "Get all airports", description = "Returns a list of all registered airports.", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT,
            HR_AND_MANAGEMENT,
            SCHEDULE_COORDINATOR
    }))
    @ApiResponses(value = {@ApiResponse(responseCode = "200", description = "Successfully retrieved the list of airports")})
    public ResponseEntity<List<AirportDTO>> getAllAirports() {
        return ResponseEntity.ok(facade.getAllAirports());
    }

    @GetMapping(value = "/{id}", produces = "application/json")
    @Operation(summary = "Get airport by ID", description = "Retrieves an airport based on its unique ID.", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT,
            HR_AND_MANAGEMENT,
            SCHEDULE_COORDINATOR
    }))
    @ApiResponses(value = {@ApiResponse(responseCode = "200", description = "Successfully found the airport"), @ApiResponse(responseCode = "404", description = "Airport not found")})
    public ResponseEntity<AirportDTO> getAirportById(@PathVariable Long id) {
        return facade.getAirportById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @PostMapping
    @Operation(summary = "Create a new airport", description = "Creates and returns a newly registered airport.", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponses(value = {@ApiResponse(responseCode = "201", description = "Airport successfully created"), @ApiResponse(responseCode = "400", description = "Invalid request data")})
    public ResponseEntity<AirportDTO> createAirport(@Valid @RequestBody AirportCreateDTO airport) {
        AirportDTO createdAirport = facade.createAirport(airport);
        return ResponseEntity.created(URI.create("/airports/" + createdAirport.getId()))
                .body(createdAirport);
    }

    @DeleteMapping("/{id}")
    @Operation(summary = "Delete an airport", description = "Removes an airport from the system by ID.", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponses(value = {@ApiResponse(responseCode = "204", description = "Airport successfully deleted"), @ApiResponse(responseCode = "404", description = "Airport not found")})
    public ResponseEntity<Void> deleteAirport(@PathVariable Long id) {
        facade.deleteAirport(id);
        return ResponseEntity.noContent().build();
    }

    @PutMapping
    @Operation(summary = "Update an airport", description = "Updates the details of an existing airport.", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponses(value = {@ApiResponse(responseCode = "200", description = "Airport successfully updated"), @ApiResponse(responseCode = "404", description = "Airport not found"), @ApiResponse(responseCode = "400", description = "Invalid request data")})
    public ResponseEntity<AirportDTO> updateAirport(@Valid @RequestBody AirportUpdateDTO airport) {
        return ResponseEntity.ok(facade.updateAirport(airport));
    }
}