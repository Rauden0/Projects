package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import io.swagger.v3.oas.annotations.media.Schema;

public record StewardDto(
        /** Steward identifier. */
        @Schema(description = "steward identifier", example = "12") Long id,
        /** Steward's given name. */
        @Schema(description = "steward's given name", example = "Rhoda") String givenName,
        /** Steward's family name. */
        @Schema(description = "steward's family name", example = "Teneiro") String familyName) {

}
