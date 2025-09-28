package cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment;

import lombok.*;

import java.io.Serializable;

@AllArgsConstructor(access = AccessLevel.PRIVATE)
@Builder
@Getter
public class StewardAssignmentDto implements Serializable {
    public static StewardAssignmentDto between(Long stewardId, Long flightId) {
        return new StewardAssignmentDto(stewardId, flightId);
    }

    @NonNull
    private final Long stewardId;

    @NonNull
    private final Long flightId;
}
