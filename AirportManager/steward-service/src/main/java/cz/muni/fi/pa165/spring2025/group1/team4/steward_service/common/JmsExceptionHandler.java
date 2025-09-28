package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.jms.JmsException;
import org.springframework.stereotype.Component;

@Component
@Slf4j
public class JmsExceptionHandler {
    @Value("${airport_manager.consistency.events.required}")
    private boolean requireSend;

    public void handleUnsentEvent(Object unsentEvent, JmsException e) {
        if (requireSend) {
            throw e;
        }

        String className = unsentEvent.getClass().getSimpleName();
        String message = e.getMessage();
        log.info("Failed to deliver message of type {}: {}", className, message);
    }
}
