package cz.muni.fi.pa165.spring2025.group1.team4.events.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;

import java.io.Serializable;

public record FlightChangeEvent(ChangeType changeType, FlightDto flight)
        implements Serializable {
    public static FlightChangeEvent createEvent(FlightDto flight) {
        return new FlightChangeEvent(ChangeType.CREATED, flight);
    }

    public static FlightChangeEvent updateEvent(FlightDto flight) {
        return new FlightChangeEvent(ChangeType.UPDATED, flight);
    }

    public static FlightChangeEvent deleteEvent(Long flightId) {
        return new FlightChangeEvent(ChangeType.DELETED,
                FlightDto.builder().id(flightId).build());
    }
}
