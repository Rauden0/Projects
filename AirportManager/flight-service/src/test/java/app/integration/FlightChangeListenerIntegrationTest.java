package app.integration;

import app.FlightServiceApplication;
import app.web.FlightChangeListener;
import app.web.FlightEventHandler;
import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.test.context.ActiveProfiles;

import static org.mockito.Mockito.spy;

@SpringBootTest(classes = FlightServiceApplication.class)
@ActiveProfiles("test")
@DirtiesContext(classMode = DirtiesContext.ClassMode.AFTER_EACH_TEST_METHOD)
@TestConfiguration
public class FlightChangeListenerIntegrationTest {

    @Autowired
    private JmsTemplate jmsTemplate;

    @Autowired
    private FlightEventHandler flightEventHandler;

    @Autowired
    private FlightChangeListener listener;

    @Test
    void testFlightChangeListenerReceivesMessage() throws Exception {
        FlightEventHandler spyHandler = spy(flightEventHandler);

        FlightDto flightDto = FlightDto.builder().id(100L).build();
        FlightChangeEvent event = new FlightChangeEvent(ChangeType.DELETED, flightDto);

        jmsTemplate.convertAndSend("flight-changes-for-steward-service", event);

        Thread.sleep(500);

    }
}
