package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JmsExceptionHandler;
import jakarta.annotation.PostConstruct;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.jms.JmsException;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Component;

import java.util.function.Consumer;

@Slf4j
@Component
@RequiredArgsConstructor
public class StewardChangeProducer implements Consumer<StewardChangeEvent> {
    private final JmsExceptionHandler handler;

    private final JmsTemplate jmsTemplate;

    private final StewardEventDispatcher dispatcher;

    @PostConstruct
    private void register() {
        dispatcher.addEventListener(this);
    }

    @Value("${airport_manager.events.change.steward.listeners}")
    private String[] destinations;

    @Override
    public void accept(StewardChangeEvent event) {
        for (String destinationName : destinations) {
            try {
                jmsTemplate.convertAndSend(destinationName, event);
            } catch (JmsException e) {
                handler.handleUnsentEvent(event, e);
            }
        }
    }
}
