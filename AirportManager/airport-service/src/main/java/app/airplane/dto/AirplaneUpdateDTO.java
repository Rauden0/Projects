package app.airplane.dto;

import app.airplane.entity.AirplaneType;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

@Data
public class AirplaneUpdateDTO {
    @NotNull
    private Long id;
    private String name;
    private AirplaneType type;
    private Integer capacity;
}
