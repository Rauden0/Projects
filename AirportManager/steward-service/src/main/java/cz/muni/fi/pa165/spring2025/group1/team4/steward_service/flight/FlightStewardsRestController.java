package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceConflictException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.SecurityConfiguration;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

import java.util.List;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.HR_AND_MANAGEMENT;
import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.SCHEDULE_COORDINATOR;

@RestController
@Tag(name = "Flight stewards", description = "Manage stewards for a given flight")
@RequestMapping("/flights/{id}/stewards")
@RequiredArgsConstructor
public class FlightStewardsRestController {

    private final FlightStewardsFacade stewardsFacade;

    @Operation(summary = "Get all of the flight's stewards", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @GetMapping
    public List<StewardDto> findAll(@Valid @PathVariable Long id) throws ResourceNotFoundException {
        return stewardsFacade.findAll(id);
    }

    @Operation(summary = "Add a steward to the flight", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @PutMapping("/{stewardId}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void add(@Valid @PathVariable Long id, @Valid @PathVariable Long stewardId)
            throws ResourceNotFoundException, ResourceConflictException {
        stewardsFacade.addFlightSteward(id, stewardId);
    }

    @Operation(summary = "Remove a steward from the flight", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @DeleteMapping("/{stewardId}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void remove(@Valid @PathVariable Long id, @Valid @PathVariable Long stewardId)
            throws ResourceNotFoundException {
        stewardsFacade.removeFlightSteward(id, stewardId);
    }

}
