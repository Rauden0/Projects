package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import org.mapstruct.Mapper;

import java.util.List;

@Mapper(componentModel = "spring")
public interface FlightStewardsMapper {

    List<StewardDto> toList(List<Steward> steward);

}
