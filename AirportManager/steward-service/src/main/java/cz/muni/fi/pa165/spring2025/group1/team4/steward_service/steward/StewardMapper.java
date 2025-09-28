package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;

import java.util.List;

@Mapper(componentModel = "spring")
public interface StewardMapper {

    StewardDto toDto(Steward steward);

    List<StewardDto> toList(List<Steward> steward);

    default Page<StewardDto> toPage(Page<Steward> stewards) {
        return new PageImpl<>(toList(stewards.getContent()), stewards.getPageable(),
                stewards.getTotalPages());
    }

    @Mapping(target = "flights", ignore = true)
    Steward toSteward(StewardDto stewardDto);

    @Mapping(target = "id", ignore = true)
    @Mapping(target = "flights", ignore = true)
    Steward toSteward(StewardNewDto stewardNewDto);
}
