package app.dto;

import app.struct.Status;
import lombok.Data;

import java.time.LocalDateTime;
import java.util.List;

@Data
public class FlightDTO {
    private Integer id;
    private String flightCode;
    private LocalDateTime departureTime;
    private LocalDateTime arrivalTime;
    private Integer price;
    private Integer availableSeats;
    private Integer totalSeats;
    private Status status;
    private Integer departureAirportId;
    private Integer arrivalAirportId;
    private Integer planeId;
    private List<Long> stewardsIds;
}
