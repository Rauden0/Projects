package app.airplane.mapper;

import app.airplane.entity.Airplane;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airplane.AirplaneDto;
import org.mapstruct.Mapper;

import java.util.List;

@Mapper(componentModel = "spring")
public interface AirplaneEventMapper {
    AirplaneDto toEventDto(Airplane airplane);

    List<AirplaneDto> toEventDtoList(List<Airplane> airplane);
}
