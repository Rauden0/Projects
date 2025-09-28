package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.facade;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.*;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;

import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class StewardFacadeTest {
    @Mock
    private StewardService service;

    @Mock
    private StewardMapper mapper;

    @InjectMocks
    private StewardFacade facade;

    private final Steward testSteward = TestData.createSteward(1L);
    private final StewardDto testDto = TestData.createStewardDto(1L);
    private final StewardNewDto testNewDto = TestData.createStewardNewDto();

    @Test
    void findById_ExistingId_ReturnsDto() {
        when(service.findById(1L)).thenReturn(Optional.of(testSteward));
        when(mapper.toDto(testSteward)).thenReturn(testDto);

        Optional<StewardDto> result = facade.findById(1L);
        assertTrue(result.isPresent());
        assertEquals("Tonda", result.get().getGivenName());
    }

    @Test
    void findAll_ReturnsPageOfDtos() {
        Pageable pageable = PageRequest.of(0, 10);
        when(service.findAll(pageable))
                .thenReturn(new PageImpl<>(List.of(testSteward)));
        when(mapper.toPage(any()))
                .thenReturn(new PageImpl<>(List.of(testDto)));

        Page<StewardDto> result = facade.findAllStewards(pageable);
        assertEquals(1, result.getTotalElements());
    }

    @Test
    void create_ValidDto_ReturnsCreatedDto() {
        when(mapper.toSteward(testNewDto)).thenReturn(testSteward);
        when(service.createSteward(testSteward)).thenReturn(testSteward);
        when(mapper.toDto(testSteward)).thenReturn(testDto);

        StewardDto result = facade.createSteward(testNewDto);
        assertEquals(1L, result.getId());
    }

    @Test
    void update_ValidDto_ReturnsUpdatedDto() throws ResourceNotFoundException {
        when(mapper.toSteward(testDto)).thenReturn(testSteward);
        when(service.updateSteward(testSteward)).thenReturn(testSteward);
        when(mapper.toDto(testSteward)).thenReturn(testDto);

        StewardDto result = facade.updateSteward(testDto);
        assertEquals("Tondovicz", result.getFamilyName());
    }

    @Test
    void delete_ExistingId_DeletesSuccessfully() {
        doNothing().when(service).deleteSteward(any());
        assertDoesNotThrow(() -> facade.deleteSteward(1L));
    }
}