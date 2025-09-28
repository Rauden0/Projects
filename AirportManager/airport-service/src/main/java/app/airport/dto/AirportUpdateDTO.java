package app.airport.dto;

import jakarta.validation.constraints.NotNull;
import lombok.Data;

@Data
public class AirportUpdateDTO {
    @NotNull
    private Long id;
    private String country;
    private String city;
    private String name;
}
