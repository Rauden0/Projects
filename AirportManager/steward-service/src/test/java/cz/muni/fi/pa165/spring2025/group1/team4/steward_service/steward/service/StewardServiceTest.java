package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.service;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.ResourceNotFoundException;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardRepository;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardService;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.TestData;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.jms.StewardEventDispatcher;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.dao.EmptyResultDataAccessException;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;

import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class StewardServiceTest {
    private final Steward testSteward = TestData.createSteward(1L);

    @Mock
    private StewardRepository repository;

    @Mock
    private StewardEventDispatcher dispatcher;

    @InjectMocks
    private StewardService service;

    @Test
    void findById_ExistingId_ReturnsSteward() {
        when(repository.findById(1L)).thenReturn(Optional.of(testSteward));
        Optional<Steward> result = service.findById(1L);
        assertTrue(result.isPresent());
        assertEquals("Tonda", result.get().getGivenName());
        verifyNoInteractions(dispatcher);
    }

    @Test
    void findById_NonExistingId_ReturnsEmpty() {
        when(repository.findById(999L)).thenReturn(Optional.empty());
        Optional<Steward> result = service.findById(999L);
        assertTrue(result.isEmpty());
        verifyNoInteractions(dispatcher);
    }

    @Test
    void findAll_ReturnsPageOfStewards() {
        Pageable pageable = PageRequest.of(0, 10);
        when(repository.findAll(pageable)).thenReturn(new PageImpl<>(List.of(testSteward)));

        Page<Steward> result = service.findAll(pageable);
        assertEquals(1, result.getTotalElements());
        verifyNoInteractions(dispatcher);
    }

    @Test
    void create_ValidSteward_ReturnsSavedSteward() {
        Steward newSteward = TestData.createSteward(null);
        when(repository.save(newSteward)).thenReturn(testSteward);

        Steward result = service.createSteward(newSteward);
        assertEquals(1L, result.getId());
        verify(dispatcher).emitCreation(testSteward);
        verifyNoMoreInteractions(dispatcher);
    }

    @Test
    void update_ExistingSteward_ReturnsUpdatedSteward() throws ResourceNotFoundException {
        when(repository.save(testSteward)).thenReturn(testSteward);

        Steward result = service.updateSteward(testSteward);

        assertEquals("Tonda", result.getGivenName());
        verify(repository).save(testSteward);
        verify(dispatcher).emitUpdate(testSteward);
        verifyNoMoreInteractions(dispatcher);
    }

    @Test
    void delete_ExistingSteward_DeletesSuccessfully() {
        doNothing().when(repository).delete(testSteward);
        assertDoesNotThrow(() -> service.deleteSteward(testSteward));
        verify(dispatcher).emitDelete(testSteward.getId());
        verifyNoMoreInteractions(dispatcher);
    }

    @Test
    void delete_NonExistingSteward_ThrowsException() {
        Steward nonExisting = TestData.createSteward(999L);
        doThrow(EmptyResultDataAccessException.class).when(repository).delete(nonExisting);
        assertThrows(EmptyResultDataAccessException.class, () -> service.deleteSteward(nonExisting));
        verifyNoInteractions(dispatcher);
    }
}
