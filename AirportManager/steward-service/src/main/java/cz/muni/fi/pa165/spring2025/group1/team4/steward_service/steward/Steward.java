package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import jakarta.persistence.*;
import lombok.*;
import lombok.EqualsAndHashCode.CacheStrategy;

import java.util.HashSet;
import java.util.Set;

/** A flight steward. */
@Entity
@Table(name = "stewards")
@NoArgsConstructor
@AllArgsConstructor(access = AccessLevel.PRIVATE)
@ToString(includeFieldNames = true)
@EqualsAndHashCode(of = { "id" }, cacheStrategy = CacheStrategy.LAZY)
public class Steward {
    public static Steward withId(Long id) {
        Steward steward = new Steward();
        steward.setId(id);
        return steward;
    }

    public static Steward named(String givenName, String familyName) {
        return new Steward(null, givenName, familyName, new HashSet<>());
    }

    /** Steward identifier. */
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id_steward")
    private @Getter @Setter Long id;

    /** Steward's given name. */
    @Column(name = "given_name", length = 50)
    private @Getter @Setter String givenName;

    /** Steward's family name. */
    @Column(name = "family_name", length = 100)
    private @Getter @Setter String familyName;

    @ManyToMany(mappedBy = "stewards", cascade = {CascadeType.PERSIST, CascadeType.MERGE, CascadeType.REMOVE})
    @ToString.Exclude
    private @Getter Set<Flight> flights = new HashSet<>();

    @ToString.Include(name = "flights")
    private int getCountFlights() {
        return this.flights.size();
    }

}
