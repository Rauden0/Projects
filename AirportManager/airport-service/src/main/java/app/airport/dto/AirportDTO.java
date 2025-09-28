package app.airport.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;

@Setter
@Getter
public class AirportDTO {
    @NotNull
    private Long id;
    @NotBlank
    private String country;
    @NotBlank
    private String city;
    @NotBlank
    private String name;
}
