package app.airplane.mapper;

import app.airplane.dto.AirplaneCreateDTO;
import app.airplane.dto.AirplaneDTO;
import app.airplane.dto.AirplaneUpdateDTO;
import app.airplane.entity.Airplane;
import org.mapstruct.*;

import java.util.List;

@Mapper(componentModel = "spring")
public interface AirplaneMapper {
    @Mapping(target = "id", ignore = true)
    Airplane toEntity(AirplaneCreateDTO airplaneCreateDTO);

    @Mapping(target = "maximumTravelDistance", ignore = true)
    Airplane toEntity(AirplaneUpdateDTO airplaneDTO);

    AirplaneDTO toDTO(Airplane airplane);

    @BeanMapping(nullValuePropertyMappingStrategy = NullValuePropertyMappingStrategy.IGNORE)
    void updateAirplaneFromDto(AirplaneUpdateDTO dto, @MappingTarget Airplane entity);

    List<AirplaneDTO> toDTOList(List<Airplane> all);
}
