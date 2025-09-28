package app.airport.mapper;

import app.airport.entity.Airport;
import cz.muni.fi.pa165.spring2025.group1.team4.events.airport.AirportDto;
import org.mapstruct.Mapper;

@Mapper(componentModel = "spring")
public interface AirportEventMapper {
    AirportDto toEventDto(Airport airport);
}
