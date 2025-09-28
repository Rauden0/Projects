package app.airplane.repository;

import app.airplane.entity.Airplane;
import app.airplane.entity.AirplaneType;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

@Repository
public interface AirplaneRepository extends JpaRepository<Airplane, Long> {
    @Query("SELECT COUNT(a) > 0 FROM Airplane a WHERE " +
            "(:name IS NULL OR a.name = :name) AND " +
            "(:type IS NULL OR a.type = :type) AND " +
            "(:capacity IS NULL OR a.capacity = :capacity) AND " +
            "a.id <> :currentAirplaneId")
    boolean existsOverlappingAirplaneExceptCurrentOne(
            @Param("currentAirplaneId") Long currentAirplaneId,
            @Param("type") AirplaneType type,
            @Param("capacity") Integer capacity,
            @Param("name") String name
    );
}
