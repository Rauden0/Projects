package app.mapper;

import app.dto.FlightCreateDTO;
import app.dto.FlightDTO;
import app.dto.FlightUpdateDTO;
import app.entity.Flight;
import org.mapstruct.*;

import java.util.List;

@Mapper(componentModel = "spring")
public interface FlightMapper {
    @Mapping(target = "stewardsIds", ignore = true)
    Flight toEntity(FlightCreateDTO flightCreateDTO);

    @Mapping(target = "stewardsIds", ignore = true)
    Flight toEntity(FlightUpdateDTO flightUpdateDTO);

    @BeanMapping(nullValuePropertyMappingStrategy = NullValuePropertyMappingStrategy.IGNORE)
    void updateFlightFromDto(FlightUpdateDTO dto, @MappingTarget Flight entity);

    FlightDTO toDTO(Flight flight);

    List<FlightDTO> toDTOList(List<Flight> flights);
}