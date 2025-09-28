package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.SecurityConfiguration;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springdoc.core.annotations.ParameterObject;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ProblemDetail;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.HR_AND_MANAGEMENT;
import static cz.muni.fi.pa165.spring2025.group1.team4.security.SecurityScopes.SCHEDULE_COORDINATOR;

@RestController
@Tag(name = "Stewards", description = "Manage stewards")
@RequestMapping(path = "/stewards", produces = MediaType.APPLICATION_JSON_VALUE)
@RequiredArgsConstructor
public class StewardRestController {

    private static final List<String> ALLOWED_SORTS = List.of("id", "givenName", "familyName");

    private static String getAllowedSorts() {
        return ALLOWED_SORTS.stream().collect(Collectors.joining(", "));
    }

    private final StewardFacade stewardFacade;

    @Operation(summary = "Get info for a given steward", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @GetMapping(path = "/{id}")
    public ResponseEntity<StewardDto> findById(@Valid @PathVariable Long id) {
        return ResponseEntity.of(stewardFacade.findById(id));
    }

    @Operation(summary = "Get all stewards", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            SCHEDULE_COORDINATOR,
            HR_AND_MANAGEMENT
    }))
    @GetMapping
    public ResponseEntity<Page<StewardDto>> findAllStewards(
            @Valid @ParameterObject Pageable pageable) {
        Set<String> sortedProperties = pageable.getSort().get().map(order -> order.getProperty())
                .collect(Collectors.toCollection(HashSet::new));
        sortedProperties.removeAll(ALLOWED_SORTS);
        if (!sortedProperties.isEmpty()) {
            return ResponseEntity.of(ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST,
                    "Page sort can only be one of " + getAllowedSorts() + ".")).build();
        }
        return ResponseEntity.ok(stewardFacade.findAllStewards(pageable));
    }

    @Operation(summary = "Create a new steward", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            HR_AND_MANAGEMENT
    }))
    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public StewardDto createSteward(@Valid @RequestBody StewardNewDto stewardDto) {
        return stewardFacade.createSteward(stewardDto);
    }

    @Operation(summary = "Edit a steward", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            HR_AND_MANAGEMENT
    }))
    @PatchMapping
    public StewardDto updateSteward(@Valid @RequestBody StewardDto stewardDto) throws ResourceNotFoundException {
        return stewardFacade.updateSteward(stewardDto);
    }

    @Operation(summary = "Delete a steward", security = @SecurityRequirement(name = SecurityConfiguration.SCHEME, scopes = {
            HR_AND_MANAGEMENT
    }))
    @DeleteMapping("/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void deleteSteward(@Valid @PathVariable Long id) {
        stewardFacade.deleteSteward(id);
    }
}
