package cz.muni.fi.pa165.spring2025.group1.team4.events.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;

import java.io.Serializable;

public record StewardChangeEvent(ChangeType changeType, StewardDto steward)
        implements Serializable {
    public static StewardChangeEvent createEvent(StewardDto steward) {
        return new StewardChangeEvent(ChangeType.CREATED, steward);
    }

    public static StewardChangeEvent updateEvent(StewardDto steward) {
        return new StewardChangeEvent(ChangeType.UPDATED, steward);
    }

    public static StewardChangeEvent deleteEvent(Long stewardId) {
        return new StewardChangeEvent(ChangeType.DELETED,
                StewardDto.builder().id(stewardId).build());
    }
}
