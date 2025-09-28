package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JmsEventDispatcher;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class StewardEventDispatcher extends JmsEventDispatcher<StewardChangeEvent> {
    private final JmsStewardMapper mapper;

    public void emitCreation(Steward steward) {
        broadcastEvent(StewardChangeEvent.createEvent(mapper.toDto(steward)));
    }

    public void emitUpdate(Steward stewardUpdates) {
        broadcastEvent(StewardChangeEvent.updateEvent(mapper.toDto(stewardUpdates)));
    }

    public void emitDelete(Long id) {
        broadcastEvent(StewardChangeEvent.deleteEvent(id));
    }

}
