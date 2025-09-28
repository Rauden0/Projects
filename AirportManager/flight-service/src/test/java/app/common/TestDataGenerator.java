package app.common;

import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.entity.Flight;
import app.struct.Status;
import lombok.AccessLevel;
import lombok.Builder;
import lombok.RequiredArgsConstructor;
import org.mapstruct.Mapper;
import org.mapstruct.factory.Mappers;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.util.stream.IntStream;

@RequiredArgsConstructor(access = AccessLevel.PRIVATE)
public class TestDataGenerator {
    @Mapper
    public static interface FlightDataMapper {
        Flight toFlight(FlightData data);

        FlightUpdateDTO toUpdateDto(FlightData data);

        FlightDTO toDto(FlightData data);

        FlightCreateDTO toCreateDTO(FlightData data);
    }

    private static FlightDataMapper internalMapper = Mappers.getMapper(FlightDataMapper.class);

    @Builder
    public static record FlightData(String flightCode, int departureAirportId,
            int arrivalAirportId, int planeId, LocalDateTime departureTime,
            LocalDateTime arrivalTime,
            Integer price, Integer totalSeats, Integer availableSeats, Status status,
            List<String> stewardsCodes) {
        private static class FlightDataBuilder {
            private FlightDataBuilder() {
                status = Status.ACTIVE;
                stewardsCodes = new ArrayList<>();
            }
        }

    }

    private static record FlightPath(int from, int to) {
    }

    private static record FlightSpan(LocalDateTime from, LocalDateTime to) {
    }

    private final Random random;

    private final LocalDateTime[] flightTimes;

    private TestDataGenerator(int seed) {
        this.random = new Random(88888888888L + seed * 1234567L);
        this.flightTimes = generateFlightTimes();
    }

    private LocalDateTime[] generateFlightTimes() {
        LocalDateTime[] flightTimes = new LocalDateTime[100];
        flightTimes[0] = LocalDateTime.of(1999, 9, 9, 9, 9);
        for (var i = 1; i < flightTimes.length; i++) {
            flightTimes[i] = flightTimes[i - 1].plusSeconds(random.nextInt(1800, 3600 * 3));
        }
        return flightTimes;
    }

    private FlightPath randomFlightPath() {
        int from = random.nextInt(1,99);
        int to = random.nextInt(100,999);
        if (to == from) {
            to++;
        }
        return new FlightPath(from, to);
    }

    private FlightSpan randomFlightSpan() {
        int indexFrom = random.nextInt(flightTimes.length - 5);
        int indexTo = indexFrom + random.nextInt(5);
        return new FlightSpan(flightTimes[indexFrom], flightTimes[indexTo]);
    }

    private char randomUppercaseLetter() {
        return (char) (random.nextInt(26) + 'A');
    }

    private String randomFlightCode() {
        return String.format("%c%c-%03d", randomUppercaseLetter(),
                randomUppercaseLetter(), random.nextInt(999));
    }

    private FlightData randomFlightData() {
        var flightCode = randomFlightCode();
        var path = randomFlightPath();
        var span = randomFlightSpan();
        var price = random.nextInt(100, 600);
        var totalSeats = random.nextInt(200, 500);
        var availableSeats = random.nextInt(0, totalSeats);
        var planeId = random.nextInt(1000);
        return FlightData
                .builder()
                .flightCode(flightCode)
                .arrivalAirportId(path.from)
                .departureAirportId(path.to)
                .arrivalTime(span.from)
                .departureTime(span.to)
                .price(price)
                .availableSeats(availableSeats)
                .totalSeats(totalSeats)
                .planeId(planeId)
                .build();
    }

    public static TestDataGenerator instance(int number) {
        return new TestDataGenerator(number);
    }

    public static <T> T getDefaultFor(Class<T> clazz) {
        return instance(0).of(clazz);
    }

    private <T> T convert(FlightData data, Class<T> type) {
        if (type.equals(Flight.class)) {
            return type.cast(internalMapper.toFlight(data));
        }
        if (type.equals(FlightDTO.class)) {
            return type.cast(internalMapper.toDto(data));
        }
        if (type.equals(FlightCreateDTO.class)) {
            return type.cast(internalMapper.toCreateDTO(data));
        }
        if (type.equals(FlightUpdateDTO.class)) {
            return type.cast(internalMapper.toUpdateDto(data));
        }
        throw new IllegalArgumentException("Cannot create flight of type " + type.getName());
    }

    public <T> T of(Class<T> clazz) {
        return convert(randomFlightData(), clazz);
    }

    @RequiredArgsConstructor(access = AccessLevel.PRIVATE)
    public static class ListDataGenerator {
        private final int items;

        public <T> List<T> instancesOf(Class<T> type) {
            return IntStream.range(0, items)
                    .mapToObj((seed) -> TestDataGenerator.instance(seed).of(type)).toList();
        }
    }

    public static <T> ListDataGenerator first(int items) {
        return new ListDataGenerator(items);
    }
}
