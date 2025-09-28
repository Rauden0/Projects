package cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;

import java.io.Serializable;

public record StewardAssignmentEvent(ChangeType changeType, StewardAssignmentDto steward)
        implements Serializable {
    public static StewardAssignmentEvent createEvent(Long stewardId, Long flightId) {
        return new StewardAssignmentEvent(ChangeType.CREATED,
                StewardAssignmentDto.between(stewardId, flightId));
    }

    public static StewardAssignmentEvent deleteEvent(Long stewardId, Long flightId) {
        return new StewardAssignmentEvent(ChangeType.DELETED,
                StewardAssignmentDto.between(stewardId, flightId));
    }
}
