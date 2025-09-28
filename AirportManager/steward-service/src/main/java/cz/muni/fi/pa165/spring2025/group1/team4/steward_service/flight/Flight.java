package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import jakarta.persistence.*;
import lombok.EqualsAndHashCode;
import lombok.EqualsAndHashCode.CacheStrategy;
import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

import java.time.LocalDateTime;
import java.util.HashSet;
import java.util.Set;

/** A flight between two destinations. */
@Entity
@Table(name = "flights")
@ToString(includeFieldNames = true)
@EqualsAndHashCode(of = { "id" }, cacheStrategy = CacheStrategy.LAZY)
public class Flight {
    public static Flight withId(Long id) {
        Flight flight = new Flight();
        flight.setId(id);
        return flight;
    }

    public static Flight spanning(LocalDateTime departureTime, LocalDateTime arrivalTime) {
        Flight flight = new Flight();
        flight.setDepartureTime(departureTime);
        flight.setArrivalTime(arrivalTime);
        return flight;
    }

    /** Flight identifier. */
    @Id
    @Column(name = "id_flight")
    private @Getter @Setter Long id;

    /** Flight departure time. */
    @Column(name = "departure_time")
    private @Getter @Setter LocalDateTime departureTime;

    /** Flight arrival time. */
    @Column(name = "arrival_time")
    private @Getter @Setter LocalDateTime arrivalTime;

    @ManyToMany(fetch = FetchType.EAGER)
    @JoinTable
    @ToString.Exclude
    private @Getter @Setter Set<Steward> stewards = new HashSet<>();

    @ToString.Include(name = "stewards")
    private int getCountStewards() {
        return this.stewards.size();
    }


}
