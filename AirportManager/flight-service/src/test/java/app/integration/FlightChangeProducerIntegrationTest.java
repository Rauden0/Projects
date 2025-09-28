package app.integration;

import app.FlightServiceApplication;
import app.web.FlightChangeProducer;
import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.core.env.Environment;
import org.springframework.jms.core.JmsTemplate;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.test.context.ActiveProfiles;

import java.util.Arrays;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest(classes = FlightServiceApplication.class)
@ActiveProfiles("test")
@DirtiesContext(classMode = DirtiesContext.ClassMode.AFTER_EACH_TEST_METHOD)
@TestConfiguration
public class FlightChangeProducerIntegrationTest {

    @Autowired
    private FlightChangeProducer producer;

    @Autowired
    private JmsTemplate jmsTemplate;

    @Autowired
    private Environment env;

    @Test
    void testFlightChangeProducerSendsMessage() {
        System.out.println("Active profiles: " + Arrays.toString(env.getActiveProfiles()));
        FlightDto flightDto = FlightDto.builder().id(1L).build();
        FlightChangeEvent event = new FlightChangeEvent(ChangeType.CREATED, flightDto);

        producer.sendFlightChange(event);

        FlightChangeEvent received = (FlightChangeEvent) jmsTemplate.receiveAndConvert("flight-changes-for-steward-service");
        assertThat(received).isNotNull();
        assertThat(received.changeType()).isEqualTo(ChangeType.CREATED);
        assertThat(received.flight().getId()).isEqualTo(1L);
    }
}
