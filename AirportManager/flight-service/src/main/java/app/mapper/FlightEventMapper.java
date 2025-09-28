package app.mapper;

import app.entity.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import org.mapstruct.Mapper;

import java.util.List;

@Mapper(componentModel = "spring")
public interface FlightEventMapper {

    FlightDto toEventDto(Flight flight);

    List<FlightDto> toEventDtoList(List<Flight> flights);
}
