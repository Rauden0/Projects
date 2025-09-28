package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.messaging;

import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.FlightEventHandler;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.FlightService;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms.JmsFlightMapper;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;


@ExtendWith(MockitoExtension.class)
class FlightEventHandlerTest {

    @Mock
    FlightService flightService;

    @Mock
    JmsFlightMapper mapper;

    @InjectMocks
    FlightEventHandler handler;

    @Test
    void getHandlerForType_created_callsCreate() {
        FlightDto dto = FlightDto.builder().id(1L).build();
        Flight flight = new Flight();
        when(mapper.toFlight(dto)).thenReturn(flight);

        handler.getHandlerForType(ChangeType.CREATED)
                .orElseThrow()
                .accept(dto);

        verify(flightService).create(flight);
    }

    @Test
    void getHandlerForType_deleted_callsDeleteById() {
        FlightDto dto = FlightDto.builder().id(1L).build();

        handler.getHandlerForType(ChangeType.DELETED)
                .orElseThrow()
                .accept(dto);

        verify(flightService).deleteById(1L);
    }

    @Test
    void getHandlerForType_updated_callsUpdate() {
        FlightDto dto = FlightDto.builder().id(1L).build();
        Flight flight = new Flight();
        when(mapper.toFlight(dto)).thenReturn(flight);

        handler.getHandlerForType(ChangeType.UPDATED)
                .orElseThrow()
                .accept(dto);

        verify(flightService).update(flight);
    }


}
