package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JpaEntityInitializer;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;

@Repository
public interface FlightRepository
        extends JpaRepository<Flight, Long>, JpaSpecificationExecutor<Flight>,
        JpaEntityInitializer<Flight> {
    @Query("""
            select
                case when count(*) > 0 then true else false end
            from
                Flight flight
            where
                :steward member of stewards
                and (:otherDeparture between departureTime and arrivalTime
                    or :otherArrival between departureTime and arrivalTime)
                        """)
    boolean existsFlightWithStewardBetweenGivenTimes(Steward steward,
            LocalDateTime otherDeparture, LocalDateTime otherArrival);
}
