package cz.muni.fi.pa165.spring2025.group1.team4.artemis_server;

import jakarta.jms.*;
import lombok.extern.slf4j.Slf4j;
import org.apache.activemq.artemis.jms.client.ActiveMQConnectionFactory;
import org.apache.activemq.artemis.junit.EmbeddedActiveMQExtension;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;

import java.io.Serializable;

import static org.assertj.core.api.Assertions.assertThat;

@Slf4j
class ArtemisServerApplicationTests {
	@RegisterExtension
	EmbeddedActiveMQExtension server = new EmbeddedActiveMQExtension();

	static record TestRecord(int value) implements Serializable {
	}

	@Test
	void testQueues() {
		server.start();
		log.info("queues: {}", server.getBoundQueues(server.getVmURL()));
	}

	@Test
	void testServer() throws JMSException {
		ConnectionFactory cf = new ActiveMQConnectionFactory(server.getVmURL());
		Connection c = cf.createConnection();
		c.start();
		Session s = c.createSession(false, Session.AUTO_ACKNOWLEDGE);
		Queue q = s.createQueue("test-record");
		MessageProducer p = s.createProducer(q);
		p.send(s.createObjectMessage(new TestRecord(35)));
		MessageConsumer consumer = s.createConsumer(q);
		Message m = consumer.receive(500);
		consumer.close();
		p.close();
		s.close();
		c.close();

		assertThat(m).isNotNull();
		assertThat(m.getBody(TestRecord.class)).isEqualTo(new TestRecord(35));
	}
}