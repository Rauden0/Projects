package cz.muni.fi.pa165.spring2025.group1.team4.events.airport;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;

import java.io.Serializable;

public record AirportChangeEvent(ChangeType changeType, AirportDto airport)
        implements Serializable {
    public static AirportChangeEvent createEvent(AirportDto airport) {
        return new AirportChangeEvent(ChangeType.CREATED, airport);
    }

    public static AirportChangeEvent updateEvent(AirportDto airport) {
        return new AirportChangeEvent(ChangeType.UPDATED, airport);
    }

    public static AirportChangeEvent deleteEvent(Long airportId) {
        return new AirportChangeEvent(ChangeType.DELETED,
                AirportDto.builder().id(airportId).build());
    }
}
