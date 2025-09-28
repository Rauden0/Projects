package cz.muni.fi.pa165.spring2025.group1.team4.events.airplane;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;

import java.io.Serializable;

public record AirplaneChangeEvent(ChangeType changeType, AirplaneDto airplane)
        implements Serializable {
    public static AirplaneChangeEvent createEvent(AirplaneDto airplane) {
        return new AirplaneChangeEvent(ChangeType.CREATED, airplane);
    }

    public static AirplaneChangeEvent updateEvent(AirplaneDto airplane) {
        return new AirplaneChangeEvent(ChangeType.UPDATED, airplane);
    }

    public static AirplaneChangeEvent deleteEvent(Long airplaneId) {
        return new AirplaneChangeEvent(ChangeType.DELETED,
                AirplaneDto.builder().id(airplaneId).build());
    }
}
