package app.airplane.controller;

import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.facade.AirplaneFacade;
import app.security.SecurityConfiguration;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.net.URI;
import java.util.List;
import java.util.logging.Logger;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.*;

@RestController
@RequestMapping("/airplanes")
@Tag(name = "Airplanes", description = "Endpoints for managing airplanes")
public class AirplaneController {
    private static final Logger logger = Logger.getLogger(AirplaneController.class.getName());
    private final AirplaneFacade facade;

    public AirplaneController(AirplaneFacade facade) {
        this.facade = facade;
    }

    @Operation(summary = "Get all airplanes", description = "Retrieves a list of all airplanes", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT,
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @ApiResponse(responseCode = "200", description = "Successfully retrieved list")
    @GetMapping(produces = "application/json")
    public ResponseEntity<List<AirplaneDTO>> getAllAirplanes() {
        logger.info("Getting all Airplanes");
        return ResponseEntity.ok(facade.getAllAirplanes());
    }

    @Operation(summary = "Get airplane by ID", description = "Retrieves an airplane by its unique ID", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT,
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @ApiResponse(responseCode = "200", description = "Airplane found")
    @ApiResponse(responseCode = "404", description = "Airplane not found")
    @GetMapping(value = "/{id}", produces = "application/json")
    public ResponseEntity<AirplaneDTO> getAirplaneById(@PathVariable Long id) {
        logger.info("Getting Airplane by ID: " + id);
        return facade.getAirplaneById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @Operation(summary = "Create a new airplane", description = "Creates a new airplane entry", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponse(responseCode = "201", description = "Airplane created successfully")
    @PostMapping
    public ResponseEntity<AirplaneDTO> createAirplane(@Valid @RequestBody AirplaneCreateDTO airplane) {
        logger.info("Creating new Airplane: " + airplane);
        AirplaneDTO createdAirplane = facade.createAirplane(airplane);
        return ResponseEntity
                .created(URI.create("/airplanes/" + createdAirplane.getId()))
                .body(createdAirplane);
    }

    @Operation(summary = "Delete an airplane", description = "Deletes an airplane by ID", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponse(responseCode = "204", description = "Airplane deleted successfully")
    @ApiResponse(responseCode = "404", description = "Airplane not found")
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteAirplane(@PathVariable Long id) {
        logger.info("Deleting Airplane: " + id);
        facade.deleteAirplane(id);
        return ResponseEntity.noContent().build();
    }

    @Operation(summary = "Update an airplane", description = "Updates the details of an existing airplane", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            COMMERCIAL_DEPT
    }))
    @ApiResponse(responseCode = "200", description = "Airplane updated successfully")
    @PutMapping
    public ResponseEntity<AirplaneDTO> updateAirplane(@RequestBody AirplaneUpdateDTO airplane) {
        logger.info("Updating Airplane: " + airplane.getId());
        return ResponseEntity.ok(facade.updateAirplane(airplane));
    }
}
