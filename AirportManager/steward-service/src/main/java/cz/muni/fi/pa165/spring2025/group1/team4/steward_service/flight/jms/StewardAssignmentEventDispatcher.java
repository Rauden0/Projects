package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JmsEventDispatcher;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class StewardAssignmentEventDispatcher extends JmsEventDispatcher<StewardAssignmentEvent> {
    public void emitCreation(Steward steward, Flight flight) {
        broadcastEvent(StewardAssignmentEvent.createEvent(steward.getId(), flight.getId()));
    }

    public void emitDelete(Steward steward, Flight flight) {
        broadcastEvent(StewardAssignmentEvent.deleteEvent(steward.getId(), flight.getId()));
    }
}
