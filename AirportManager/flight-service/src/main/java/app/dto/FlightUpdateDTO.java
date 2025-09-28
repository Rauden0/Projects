package app.dto;

import app.struct.Status;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

import java.time.LocalDateTime;


@Data
public class FlightUpdateDTO {
    @NotNull(message = "Flight ID must not be null")
    private Long id;
    private String flightCode;
    private LocalDateTime departureTime;
    private LocalDateTime arrivalTime;
    private Integer price;
    private Integer availableSeats;
    private Integer totalSeats;
    private Status status;
    private Long departureAirportId;
    private Long arrivalAirportId;
    private Long planeId;
}
