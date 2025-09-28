package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.messaging;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.FlightChangeListener;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.FlightEventHandler;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Optional;
import java.util.function.Consumer;

import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class FlightChangeListenerTest {

    @Mock
    FlightEventHandler handler;

    @Mock
    Consumer<FlightDto> consumer;

    @InjectMocks
    FlightChangeListener listener;

    @Test
    void accept_callsHandlerIfPresent() {
        FlightDto dto = FlightDto.builder().id(1L).build();
        FlightChangeEvent event = new FlightChangeEvent(ChangeType.CREATED, dto);

        when(handler.getHandlerForType(ChangeType.CREATED)).thenReturn(Optional.of(consumer));

        listener.accept(event);

        verify(consumer).accept(dto);
    }

    @Test
    void accept_doesNothingIfHandlerMissing() {
        FlightChangeEvent event = new FlightChangeEvent(ChangeType.UPDATED, FlightDto.builder().id(1L).build());

        when(handler.getHandlerForType(ChangeType.UPDATED)).thenReturn(Optional.empty());

        listener.accept(event);

        verifyNoInteractions(consumer);
    }
}
