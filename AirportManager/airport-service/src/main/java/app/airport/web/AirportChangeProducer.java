package app.airport.web;

import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportChangeEvent;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Service;

@Service
public class AirportChangeProducer {
    private final JmsTemplate jmsTemplate;

    public AirportChangeProducer(JmsTemplate jmsTemplate) {
        this.jmsTemplate = jmsTemplate;
    }

    public void sendAirportChange(AirportChangeEvent event) {
        jmsTemplate.convertAndSend("airport-changes-for-flight-service", event);
    }
}
