package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.steward;

import cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common.JpaEntityInitializer;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;
import org.springframework.stereotype.Repository;

@Repository
public interface StewardRepository
        extends JpaRepository<Steward, Long>, JpaSpecificationExecutor<Steward>,
        JpaEntityInitializer<Steward> {
}
