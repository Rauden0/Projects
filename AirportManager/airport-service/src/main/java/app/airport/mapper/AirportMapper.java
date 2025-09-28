package app.airport.mapper;

import app.airport.dto.AirportCreateDTO;
import app.airport.dto.AirportDTO;
import app.airport.dto.AirportUpdateDTO;
import app.airport.entity.Airport;
import org.mapstruct.*;

import java.util.List;

@Mapper(componentModel = "spring")
public interface AirportMapper {
    @Mapping(target = "id", ignore = true)
    Airport toEntity(AirportCreateDTO airportCreateDTO);

    @Mapping(target = "capacity", ignore = true)
    Airport toEntity(AirportUpdateDTO airportDTO);

    AirportDTO toDTO(Airport airport);

    @BeanMapping(nullValuePropertyMappingStrategy = NullValuePropertyMappingStrategy.IGNORE)
    void updateAirportFromDto(AirportUpdateDTO dto, @MappingTarget Airport entity);

    List<AirportDTO> toDTOList(List<Airport> all);
}
