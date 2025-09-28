package app.airplane.dto;

import app.airplane.entity.AirplaneType;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class AirplaneCreateDTO {
    @NotBlank
    private String name;
    @NotNull
    private AirplaneType type;
    @NotNull
    private Integer capacity;
    @NotNull
    private Integer maximumTravelDistance;
}
