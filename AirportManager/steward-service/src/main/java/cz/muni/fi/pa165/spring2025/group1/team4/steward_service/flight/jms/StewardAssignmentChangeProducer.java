package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.steward_assignment.StewardAssignmentEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JmsExceptionHandler;
import jakarta.annotation.PostConstruct;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.jms.JmsException;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Component;

import java.util.function.Consumer;

@Component
@RequiredArgsConstructor
public class StewardAssignmentChangeProducer implements Consumer<StewardAssignmentEvent> {
    private final JmsExceptionHandler exceptionHandler;

    private final JmsTemplate jmsTemplate;

    private final StewardAssignmentEventDispatcher dispatcher;

    @PostConstruct
    private void register() {
        dispatcher.addEventListener(this);
    }

    @Value("${airport_manager.events.other.stewardAssignment.listeners}")
    private String[] destinations;

    @Override
    public void accept(StewardAssignmentEvent event) {
        for (String destinationName : destinations) {
            try {
                jmsTemplate.convertAndSend(destinationName, event);
            } catch (JmsException e) {
                exceptionHandler.handleUnsentEvent(event, e);
            }
        }
    }
}
