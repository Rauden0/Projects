package cz.muni.fi.pa165.spring2025.group1.team4.artemis_server;

import lombok.RequiredArgsConstructor;
import org.apache.activemq.artemis.api.core.QueueConfiguration;
import org.apache.activemq.artemis.api.core.RoutingType;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.jms.artemis.ArtemisConfigurationCustomizer;
import org.springframework.boot.autoconfigure.jms.artemis.ArtemisProperties;
import org.springframework.context.annotation.Configuration;

@Configuration
@RequiredArgsConstructor
public class ArtemisConfig implements ArtemisConfigurationCustomizer {

    @Value("${airport_manager.messaging.queues}")
    private final String[] QUEUES = {};

    private final ArtemisProperties artemisProperties;

    @Override
    public void customize(org.apache.activemq.artemis.core.config.Configuration configuration) {
        try {
            configuration.setSecurityEnabled(false);
            configuration.addAcceptorConfiguration("netty", artemisProperties.getBrokerUrl());
            for (String queue : QUEUES) {
                configuration.addQueueConfiguration(createQueueConfiguration(queue));
            }
        } catch (Exception e) {
            throw new RuntimeException("Could not start Artemis JMS", e);
        }
    }

    private QueueConfiguration createQueueConfiguration(String queue) {
        return QueueConfiguration.of(queue)
                .setRoutingType(RoutingType.ANYCAST)
                .setMaxConsumers(1);
    }
}
