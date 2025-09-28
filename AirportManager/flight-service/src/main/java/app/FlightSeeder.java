package app;

import app.entity.Airplane;
import app.entity.Airport;
import app.entity.Flight;
import app.repository.AirplaneRepository;
import app.repository.AirportRepository;
import app.repository.FlightRepository;
import app.struct.Status;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;

@Component
public class FlightSeeder implements CommandLineRunner {

    private final FlightRepository flightRepository;
    private final AirportRepository airportRepository;
    private final AirplaneRepository airplaneRepository;

    public FlightSeeder(FlightRepository flightRepository, AirportRepository airportRepository, AirplaneRepository airplaneRepository) {
        this.flightRepository = flightRepository;
        this.airportRepository = airportRepository;
        this.airplaneRepository = airplaneRepository;
    }

    @Value("${seedData}")
    private boolean seedData;

    @Override
    public void run(String... args) {
        // Check if data already exists to avoid reseeding
        if (flightRepository.count() == 0 && seedData) {
            seedFlights();
        }
    }

    private void seedFlights() {
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");

        Flight flight1 = Flight.builder()
                .flightCode("FL123")
                .departureAirportId(1L)
                .arrivalAirportId(2L)
                .departureTime(LocalDateTime.parse("2025-05-01 08:00:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-01 11:00:00", formatter))
                .price(300)
                .availableSeats(50)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(1L)
                .build();

        Flight flight2 = Flight.builder()
                .flightCode("FL124")
                .departureAirportId(2L)
                .arrivalAirportId(3L)
                .departureTime(LocalDateTime.parse("2025-05-02 10:00:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-02 11:30:00", formatter))
                .price(150)
                .availableSeats(60)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(2L)
                .build();

        Flight flight3 = Flight.builder()
                .flightCode("FL125")
                .departureAirportId(3L)
                .arrivalAirportId(1L)
                .departureTime(LocalDateTime.parse("2025-05-03 12:00:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-03 14:00:00", formatter))
                .price(250)
                .availableSeats(70)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(3L)
                .build();

        Flight flight4 = Flight.builder()
                .flightCode("FL126")
                .departureAirportId(1L)
                .arrivalAirportId(4L)
                .departureTime(LocalDateTime.parse("2025-05-04 09:00:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-04 13:00:00", formatter))
                .price(350)
                .availableSeats(40)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(4L)
                .build();

        Flight flight5 = Flight.builder()
                .flightCode("FL127")
                .departureAirportId(4L)
                .arrivalAirportId(2L)
                .departureTime(LocalDateTime.parse("2025-05-05 07:30:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-05 10:00:00", formatter))
                .price(280)
                .availableSeats(65)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(5L)
                .build();

        Flight flight6 = Flight.builder()
                .flightCode("FL128")
                .departureAirportId(2L)
                .arrivalAirportId(3L)
                .departureTime(LocalDateTime.parse("2025-05-06 15:00:00", formatter))
                .arrivalTime(LocalDateTime.parse("2025-05-06 18:00:00", formatter))
                .price(200)
                .availableSeats(55)
                .totalSeats(100)
                .status(Status.ACTIVE)
                .planeId(6L)
                .build();

        flight1.setStewardsIds(List.of(1L, 2L));
        flight2.setStewardsIds(List.of(3L, 4L));
        flight3.setStewardsIds(List.of(5L, 6L));

        flightRepository.saveAll(List.of(flight1, flight2, flight3, flight4, flight5, flight6));


        List<Airport> airports = airportRepository.saveAll(List.of(
                Airport.builder().id(1L).build(),
                Airport.builder().id(2L).build(),
                Airport.builder().id(3L).build(),
                Airport.builder().id(4L).build()
        ));
        List<Airplane> airplanes = airplaneRepository.saveAll(List.of(
                Airplane.builder().id(1L).build(),
                Airplane.builder().id(2L).build(),
                Airplane.builder().id(3L).build(),
                Airplane.builder().id(4L).build(),
                Airplane.builder().id(5L).build()
        ));

    }

}
