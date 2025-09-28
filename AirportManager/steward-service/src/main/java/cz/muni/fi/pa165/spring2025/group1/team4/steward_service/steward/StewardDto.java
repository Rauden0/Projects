package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;

public class StewardDto {
    /** Steward identifier. */
    @NotNull
    @Schema(description = "steward identifier", example = "3")
    private @Getter @Setter Long id;

    /** Steward's given name. Required. */
    @NotBlank
    @Schema(description = "steward's given name", example = "Rhoda")
    private @Getter @Setter String givenName;

    /** Steward's family name. */
    @NotNull
    @Schema(description = "steward's family name", example = "Teneiro")
    private @Getter @Setter String familyName;
}
