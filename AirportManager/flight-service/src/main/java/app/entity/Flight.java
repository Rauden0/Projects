package app.entity;

import app.struct.Status;
import jakarta.persistence.*;
import lombok.*;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

@Entity
@Table(name = "flights")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class Flight {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false, unique = true)
    private String flightCode;

    @Column(nullable = false)
    private Long departureAirportId;
    @Column(nullable = false)
    private Long arrivalAirportId;
    @Column(nullable = false)
    private Long planeId;
    @Column(nullable = false)
    private LocalDateTime departureTime;
    @Column(nullable = false)
    private LocalDateTime arrivalTime;
    @Column(nullable = false)
    private Integer price = 0;
    @Column(nullable = false)
    private Integer availableSeats = 0;
    @Column(nullable = false)
    private Integer totalSeats = 0;
    @Enumerated(EnumType.STRING)
    private Status status = Status.ACTIVE;

    @ElementCollection
    @CollectionTable(name = "flight_stewards", joinColumns = @JoinColumn(name = "flight_id")
            , uniqueConstraints = @UniqueConstraint(columnNames = "steward_id")
    )
    @Column(name = "steward_id")
    @Builder.Default
    private List<Long> stewardsIds = new ArrayList<>();

}


