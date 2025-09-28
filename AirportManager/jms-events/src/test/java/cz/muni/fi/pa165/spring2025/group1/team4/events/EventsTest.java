package cz.muni.fi.pa165.spring2025.group1.team4.events;

import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneDto;
import cz.muni.fi.pa165.spring2025.group1.team4.events.common.ChangeType;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardChangeEvent;
import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardDto;
import org.junit.jupiter.api.Test;
import org.mapstruct.Mapper;
import org.mapstruct.ReportingPolicy;
import org.mapstruct.factory.Mappers;

import static org.assertj.core.api.Assertions.assertThat;

public class EventsTest {
    record DummyEntity(Long id) {
    }

    @Mapper(unmappedTargetPolicy = ReportingPolicy.IGNORE)
    interface DummyMapper {
        static DummyMapper getInstance() {
            return Mappers.getMapper(DummyMapper.class);
        }

        AirplaneDto toAirplaneDto(DummyEntity dummyEntity);

        StewardDto toStewardDto(DummyEntity dummyEntity);
    }

    DummyMapper mapper = DummyMapper.getInstance();

    @Test
    void testCreateEvent() {
        DummyEntity dummyEntity = new DummyEntity(31L);

        AirplaneDto airplane = mapper.toAirplaneDto(dummyEntity);

        AirplaneChangeEvent event = AirplaneChangeEvent.createEvent(airplane);

        assertThat(event).isNotNull();
        assertThat(event.airplane().getId()).isEqualTo(31L);
        assertThat(event.changeType()).isEqualTo(ChangeType.CREATED);
    }

    @Test
    void testDeleteEvent() {
        StewardChangeEvent event = StewardChangeEvent.deleteEvent(22L);

        assertThat(event).isNotNull();
        assertThat(event.changeType()).isEqualTo(ChangeType.DELETED);
        assertThat(event.steward().getId()).isEqualTo(22L);
    }
}
