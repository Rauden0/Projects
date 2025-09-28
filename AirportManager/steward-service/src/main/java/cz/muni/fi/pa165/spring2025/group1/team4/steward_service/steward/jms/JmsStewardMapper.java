package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms;

import cz.muni.fi.pa165.spring2025.group1.team4.events.steward.StewardDto;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import org.mapstruct.Mapper;
import org.mapstruct.ReportingPolicy;

@Mapper(componentModel = "spring", unmappedTargetPolicy = ReportingPolicy.IGNORE)
public interface JmsStewardMapper {
    StewardDto toDto(Steward steward);
}
