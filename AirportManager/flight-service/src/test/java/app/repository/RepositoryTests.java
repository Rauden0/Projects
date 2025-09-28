package app.repository;

import app.entity.Airplane;
import app.entity.Airport;
import app.entity.Flight;
import app.struct.Status;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;

@DataJpaTest
class RepositoryTests {

    @Autowired
    private AirplaneRepository airplaneRepository;

    @Autowired
    private AirportRepository airportRepository;

    @Autowired
    private FlightRepository flightRepository;

    @Test
    @DisplayName("Test Airplane Save and Find")
    void testAirplaneSaveAndFind() {
        Airplane airplane = new Airplane(0L);
        airplane = airplaneRepository.save(airplane);

        Optional<Airplane> found = airplaneRepository.findById(airplane.getId());
        assertThat(found).isPresent();
        assertThat(found.get().getId()).isEqualTo(0L);
    }

    @Test
    @DisplayName("Test Airplane Find All")
    void testAirplaneFindAll() {
        airplaneRepository.save(new Airplane(0L));
        airplaneRepository.save(new Airplane(1L));

        List<Airplane> airplanes = airplaneRepository.findAll();
        assertThat(airplanes.size()).isEqualTo(2);
    }

    @Test
    @DisplayName("Test Airport Save and Find")
    void testAirportSaveAndFind() {
        Airport airport = new Airport(0L);
        airport = airportRepository.save(airport);

        Optional<Airport> found = airportRepository.findById(airport.getId());
        assertThat(found).isPresent();
        assertThat(found.get().getId()).isEqualTo(0L);
    }

    @Test
    @DisplayName("Test Airport Find All")
    void testAirportFindAll() {
        airportRepository.save(new Airport(0L));
        airportRepository.save(new Airport(1L));

        List<Airport> airports = airportRepository.findAll();
        assertThat(airports.size()).isEqualTo(2);
    }

    @Test
    @DisplayName("Test Airplane Delete")
    void testAirplaneDelete() {
        Airplane airplane = airplaneRepository.save(new Airplane(0L));
        airplaneRepository.deleteById(airplane.getId());

        Optional<Airplane> found = airplaneRepository.findById(airplane.getId());
        assertThat(found).isNotPresent();
    }

    @Test
    @DisplayName("Test Airport Delete")
    void testAirportDelete() {
        Airport airport = airportRepository.save(new Airport(0L));
        airportRepository.deleteById(airport.getId());

        Optional<Airport> found = airportRepository.findById(airport.getId());
        assertThat(found).isNotPresent();
    }

    @Test
    @DisplayName("Test Flight Save and Find")
    void testFlightSaveAndFind() {
        Flight flight = saveSampleFlight();

        Optional<Flight> found = flightRepository.findById(flight.getId());
        assertThat(found).isPresent();
        assertThat(found.get().getFlightCode()).isEqualTo("NY-LA-001");
    }

    @Test
    @DisplayName("Test Find Flight By Status")
    void testFindFlightByStatus() {
        saveSampleFlight();

        Page<Flight> flights = flightRepository.findByStatus(Status.ACTIVE, Pageable.unpaged());
        assertThat(flights).isNotEmpty();
    }

    @Test
    @DisplayName("Test Find Flights With Available Seats")
    void testFindFlightsWithAvailableSeats() {
        saveSampleFlight();

        Page<Flight> flights = flightRepository.findByAvailableSeatsGreaterThan(10, Pageable.unpaged());
        assertThat(flights).isNotEmpty();
    }

    @Test
    @DisplayName("Test Find Flight By Flight Code")
    void testFindFlightByFlightCode() {
        saveSampleFlight();

        Optional<Flight> flight = flightRepository.findByFlightCode("NY-LA-001");
        assertThat(flight).isPresent();
    }

    @Test
    @DisplayName("Test Exists Overlapping Flight")
    void testExistsOverlappingFlight() {
        Flight flight = saveSampleFlight();

        boolean overlapExists = flightRepository.existsOverlappingFlight(
                flight.getPlaneId(),
                flight.getDepartureTime().minusHours(1),
                flight.getArrivalTime().plusHours(1)
        );

        assertThat(overlapExists).isTrue();
    }

    @Test
    @DisplayName("Test Delete Flight By Airport IDs")
    void testDeleteByDepartureOrArrivalAirportId() {
        Flight flight = saveSampleFlight();

        flightRepository.deleteByDepartureAirportIdOrArrivalAirportId(
                flight.getDepartureAirportId(), flight.getArrivalAirportId());

        Optional<Flight> found = flightRepository.findById(flight.getId());
        assertThat(found).isNotPresent();
    }

    @Test
    @DisplayName("Test Delete Flight By Plane ID")
    void testDeleteByPlaneId() {
        Flight flight = saveSampleFlight();

        flightRepository.deleteByPlaneId(flight.getPlaneId());

        Optional<Flight> found = flightRepository.findById(flight.getId());
        assertThat(found).isNotPresent();
    }

    @Test
    @DisplayName("Test Get Current Flights")
    void testGetCurrentFlights() {
        saveSampleFlight();
        Page<Flight> flights = flightRepository.getCurrentFlights(null, Pageable.unpaged());
        assertThat(flights).isNotEmpty();
    }

    @Test
    @Transactional
    @DisplayName("Test Find Flight With Stewards")
    void testFindByFlightCodeWithStewards() {
        saveSampleFlight();

        Optional<Flight> flight = flightRepository.findByFlightCodeWithStewards("NY-LA-001");
        assertThat(flight).isPresent();
    }

    private Flight saveSampleFlight() {
        Airplane airplane = new Airplane(0L);
        airplane = airplaneRepository.save(airplane);

        Airport departure = new Airport(1L);
        departure = airportRepository.save(departure);

        Airport arrival = new Airport(2L);
        arrival = airportRepository.save(arrival);

        Flight flight = new Flight();
        flight.setFlightCode("NY-LA-001");
        flight.setPrice(1);
        flight.setTotalSeats(1);
        flight.setPlaneId(airplane.getId());
        flight.setDepartureAirportId(departure.getId());
        flight.setArrivalAirportId(arrival.getId());
        flight.setDepartureTime(LocalDateTime.now().plusDays(1));
        flight.setArrivalTime(LocalDateTime.now().plusDays(1).plusHours(6));
        flight.setStatus(Status.ACTIVE);
        flight.setAvailableSeats(150);

        return flightRepository.save(flight);
    }
}