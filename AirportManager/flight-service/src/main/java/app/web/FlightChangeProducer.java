package app.web;

import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Service;

@Service
public class FlightChangeProducer {
    private final JmsTemplate jmsTemplate;

    public FlightChangeProducer(JmsTemplate jmsTemplate) {
        this.jmsTemplate = jmsTemplate;
    }

    public void sendFlightChange(FlightChangeEvent event) {
        jmsTemplate.convertAndSend("flight-changes-for-steward-service", event);
    }
}
