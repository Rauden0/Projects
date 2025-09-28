package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

public class TestData {
    public static Steward createSteward(Long id) {
        Steward steward = Steward.named("Tonda", "Tondovicz");
        if (id != null) {
            steward.setId(id);
        }
        return steward;
    }

    public static StewardDto createStewardDto(Long id) {
        StewardDto dto = new StewardDto();
        dto.setId(id);
        dto.setGivenName("Tonda");
        dto.setFamilyName("Tondovicz");
        return dto;
    }

    public static StewardNewDto createStewardNewDto() {
        StewardNewDto dto = new StewardNewDto();
        dto.setGivenName("Tonda");
        dto.setFamilyName("Tondovicz");
        return dto;
    }

    public static StewardNewDto createStewardNewDto(String givenName, String familyName) {
        StewardNewDto dto = new StewardNewDto();
        dto.setGivenName(givenName);
        dto.setFamilyName(familyName);
        return dto;
    }
}