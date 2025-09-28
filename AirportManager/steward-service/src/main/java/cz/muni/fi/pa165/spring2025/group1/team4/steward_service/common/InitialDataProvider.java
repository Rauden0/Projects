package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.Flight;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.flight.FlightRepository;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.Steward;
import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward.StewardRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;

@Component
@Transactional
@RequiredArgsConstructor
public class InitialDataProvider implements CommandLineRunner {

    private final StewardRepository stewardRepository;
    private final FlightRepository flightRepository;
    @Value("${seedData}")
    private boolean seedData;
    @Override
    @Transactional
    public void run(String... args) {
        if (stewardRepository.count() > 0 || !seedData) return;

        Steward s1 = Steward.named("Ronald", "McDonald");
        Steward s2 = Steward.named("Cammy", "Meele");
        Steward s3 = Steward.named("Grumble", "McBumble");
        Steward s4 = Steward.named("Lara", "Croft");
        Steward s5 = Steward.named("Gordon", "Freeman");
        Steward s6 = Steward.named("Jill", "Valentine");

        stewardRepository.saveAll(List.of(s1, s2, s3, s4, s5, s6));

        // Seed matching flights from FlightSeeder
        Flight f1 = new Flight();
        f1.setId(1L);
        f1.setDepartureTime(LocalDateTime.of(2025, 5, 1, 8, 0));
        f1.setArrivalTime(LocalDateTime.of(2025, 5, 1, 11, 0));
        f1.getStewards().addAll(List.of(s1, s2));

        Flight f2 = new Flight();
        f2.setId(2L);
        f2.setDepartureTime(LocalDateTime.of(2025, 5, 2, 10, 0));
        f2.setArrivalTime(LocalDateTime.of(2025, 5, 2, 11, 30));
        f2.getStewards().addAll(List.of(s3, s4));

        Flight f3 = new Flight();
        f3.setId(3L);
        f3.setDepartureTime(LocalDateTime.of(2025, 5, 3, 12, 0));
        f3.setArrivalTime(LocalDateTime.of(2025, 5, 3, 14, 0));
        f3.getStewards().addAll(List.of(s5, s6));

        flightRepository.saveAll(List.of(f1, f2, f3));
    }
}
