package app.airplane.entity;

import jakarta.persistence.*;
import lombok.*;

@Entity
@Table(name = "airplanes")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor(access = AccessLevel.PRIVATE)
@Builder
public class Airplane {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    private String name;

    @Enumerated(EnumType.STRING)
    private AirplaneType type;

    private Integer capacity;

    @Column(updatable = false)
    private Integer maximumTravelDistance;
}
