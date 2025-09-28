package app;

import app.airplane.entity.Airplane;
import app.airplane.entity.AirplaneType;
import app.airplane.repository.AirplaneRepository;
import app.airport.entity.Airport;
import app.airport.repository.AirportRepository;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class Seeder implements CommandLineRunner {
    private final AirportRepository airportRepository;
    private final AirplaneRepository airplaneRepository;


    public Seeder(AirportRepository repository, AirplaneRepository airplaneRepository) {
        this.airportRepository = repository;
        this.airplaneRepository = airplaneRepository;
    }
    @Value("${seedData}")
    private boolean seedData;

    @Override
    public void run(String... args) {
        if (airportRepository.count() == 0 && seedData ) {
            Airport airportA = Airport.builder()
                    .name("JFK")
                    .capacity(80)
                    .city("New York")
                    .country("United States")
                    .build();

            Airport airportB = Airport.builder()
                    .name("LAX")
                    .capacity(32)
                    .city("San Francisco")
                    .country("United States")
                    .build();

            Airport airportC = Airport.builder()
                    .name("BEL")
                    .capacity(61)
                    .city("Washington DC")
                    .country("United States")
                    .build();

            airportRepository.saveAll(List.of(airportA, airportB, airportC));
        }
        if (airplaneRepository.count() == 0 && seedData) {
            Airplane boeing = Airplane.builder()
                    .name("Boeing 737")
                    .type(AirplaneType.COMMERCIAL)
                    .capacity(100)
                    .maximumTravelDistance(3000)
                    .build();

            Airplane airbus = Airplane.builder()
                    .name("Airbus A320")
                    .type(AirplaneType.COMMERCIAL)
                    .capacity(120)
                    .maximumTravelDistance(3200)
                    .build();

            Airplane embraer = Airplane.builder()
                    .name("Embraer E190")
                    .type(AirplaneType.REGIONAL)
                    .capacity(90)
                    .maximumTravelDistance(2800)
                    .build();

            airplaneRepository.saveAll(List.of(boeing, airbus, embraer));
        }

    }
}
