package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.messaging;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms.JmsStewardMapper;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms.StewardEventDispatcher;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Captor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.function.Consumer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
public class StewardEventDispatcherTest {
    @Mock
    Consumer<StewardChangeEvent> handler;

    @Mock
    Consumer<StewardChangeEvent> handler2;

    @Captor
    ArgumentCaptor<StewardChangeEvent> events;

    @Mock
    JmsStewardMapper mapper;

    @InjectMocks
    StewardEventDispatcher dispatcher;

    @BeforeEach
    void setup() {
        dispatcher.addEventListener(handler);
        dispatcher.addEventListener(handler2);
    }

    @Test
    void testEventReceived_OnEmitCreation() {
        Steward steward = Steward.named("Juan", "Tuu");
        steward.setId(44L);

        StewardDto stewardDto = StewardDto.builder().id(44L).build();

        when(mapper.toDto(steward)).thenReturn(stewardDto);

        dispatcher.emitCreation(steward);

        verify(handler).accept(events.capture());
        verify(handler2).accept(events.capture());

        assertThat(events.getAllValues()).allSatisfy((StewardChangeEvent event) -> {
            assertThat(event.changeType()).isEqualTo(ChangeType.CREATED);
            assertThat(event.steward()).isEqualTo(stewardDto);
        });
    }

    @Test
    void testEventReceived_OnEmitUpdate() {
        Steward steward = Steward.named("Juan", "Tuu");
        steward.setId(44L);

        StewardDto stewardDto = StewardDto.builder().id(44L).build();

        when(mapper.toDto(steward)).thenReturn(stewardDto);

        dispatcher.emitUpdate(steward);

        verify(handler).accept(events.capture());
        verify(handler2).accept(events.capture());

        assertThat(events.getAllValues()).allSatisfy((StewardChangeEvent event) -> {
            assertThat(event.changeType()).isEqualTo(ChangeType.UPDATED);
            assertThat(event.steward()).isEqualTo(stewardDto);
        });
    }

    @Test
    void testEventReceived_OnEmitDelete() {
        dispatcher.emitDelete(44L);

        verify(handler).accept(events.capture());
        verify(handler2).accept(events.capture());

        assertThat(events.getAllValues()).allSatisfy((StewardChangeEvent event) -> {
            assertThat(event.changeType()).isEqualTo(ChangeType.DELETED);
            assertThat(event.steward().getId()).isEqualTo(44L);
        });
    }
}
