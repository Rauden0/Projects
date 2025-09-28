package app.repository;

import app.entity.Flight;
import app.struct.Status;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
public interface FlightRepository extends JpaRepository<Flight, Long>, JpaSpecificationExecutor<Flight> {

    Page<Flight> findByStatus(Status status, Pageable pageable);

    Page<Flight> findByAvailableSeatsGreaterThan(Integer availableSeats, Pageable pageable);

    Optional<Flight> findByFlightCode(String flightCode);

    @Query("SELECT f FROM Flight f WHERE f.departureTime > CURRENT_TIMESTAMP " +
            "AND (:departureAirport IS NULL OR f.departureAirportId = :departureAirport)")
    Page<Flight> getCurrentFlights(@Param("departureAirport") Long departureAirport, Pageable pageable);


    @Modifying(clearAutomatically = true,
            flushAutomatically = true)
    @Query(value = "INSERT INTO flight_stewards (flight_id, steward_id) " +
            "SELECT f.id, :steward_id FROM flights f WHERE f.id = :flight_id", nativeQuery = true)
    void addSteward(@Param("flight_id") Long flightId, @Param("steward_id") Long stewardId);


    @Query("SELECT COUNT(f) > 0 FROM Flight f WHERE f.planeId = :planeId AND " +
            "f.departureTime < :newArrival AND :newDeparture < f.arrivalTime")
    boolean existsOverlappingFlight(@Param("planeId") Long planeId,
                                    @Param("newDeparture") LocalDateTime newDeparture,
                                    @Param("newArrival") LocalDateTime newArrival
    );

    @Query("SELECT COUNT(f) > 0 FROM Flight f WHERE f.planeId = :planeId AND " +
            "f.departureTime < :newArrival AND :newDeparture < f.arrivalTime AND f.id <> :currentFlightId")
    boolean existsOverlappingFlightExceptCurrentOne(@Param("planeId") Long planeId,
                                    @Param("newDeparture") LocalDateTime newDeparture,
                                    @Param("newArrival") LocalDateTime newArrival,
                                    @Param("currentFlightId") Long currentFlightId
    );

    void deleteByDepartureAirportIdOrArrivalAirportId(Long departureAirportId, Long arrivalAirportId);
    void deleteByPlaneId(Long airplaneId);

    @Query("SELECT f FROM Flight f LEFT JOIN FETCH f.stewardsIds WHERE f.flightCode = :code")
    Optional<Flight> findByFlightCodeWithStewards(@Param("code") String code);

    @Query("SELECT f FROM Flight f WHERE :stewardId MEMBER OF f.stewardsIds")
    List<Flight> findFlightsByStewardId(Long stewardId);

}
