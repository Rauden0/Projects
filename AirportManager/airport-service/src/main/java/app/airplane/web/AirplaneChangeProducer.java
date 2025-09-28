package app.airplane.web;

import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Service;

@Service
public class AirplaneChangeProducer {
    private final JmsTemplate jmsTemplate;

    public AirplaneChangeProducer(JmsTemplate jmsTemplate) {
        this.jmsTemplate = jmsTemplate;
    }

    public void sendAirplaneChange(AirplaneChangeEvent event) {
        jmsTemplate.convertAndSend("airplane-changes-for-flight-service", event);
    }
}
