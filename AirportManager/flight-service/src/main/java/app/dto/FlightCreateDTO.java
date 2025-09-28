package app.dto;

import app.struct.Status;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

import java.time.LocalDateTime;

@Data
public class FlightCreateDTO {
    @NotNull(message = "Departure airport is mandatory")
    private Long departureAirportId;

    @NotNull(message = "Arrival airport is mandatory")
    private Long arrivalAirportId;

    @NotNull(message = "Plane is mandatory")
    private Long planeId;

    @NotBlank(message = "Flight code is mandatory")
    private String flightCode;

    @NotNull(message = "Departure time is mandatory")
    private LocalDateTime departureTime;

    @NotNull(message = "Arrival time is mandatory")
    private LocalDateTime arrivalTime;

    @NotNull(message = "Price is mandatory")
    private Integer price;

    @NotNull(message = "Available seats are mandatory")
    private Integer availableSeats;

    @NotNull(message = "Total seats are mandatory")
    private Integer totalSeats;

    @NotNull(message = "Status is mandatory")
    private Status status;
}
