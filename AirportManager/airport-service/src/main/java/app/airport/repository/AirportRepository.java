package app.airport.repository;

import app.airport.entity.Airport;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

@Repository
public interface AirportRepository extends JpaRepository<Airport, Long> {
    @Query("SELECT COUNT(a) > 0 FROM Airport a WHERE " +
            "(:country IS NULL OR a.country = :country) AND " +
            "(:city IS NULL OR a.city = :city) AND " +
            "(:name IS NULL OR a.name = :name) AND " +
            "a.id <> :currentAirportId")
    boolean existsOverlappingAirportExceptCurrentOne(
            @Param("currentAirportId") Long currentAirportId,
            @Param("country") String country,
            @Param("city") String city,
            @Param("name") String name
    );
}
