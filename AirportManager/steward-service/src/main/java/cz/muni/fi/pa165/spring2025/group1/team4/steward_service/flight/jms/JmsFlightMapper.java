package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.flight.FlightDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;

@Mapper(componentModel = "spring")
public interface JmsFlightMapper {
    @Mapping(target = "stewards", ignore = true)
    Flight toFlight(FlightDto flightDto);
}
