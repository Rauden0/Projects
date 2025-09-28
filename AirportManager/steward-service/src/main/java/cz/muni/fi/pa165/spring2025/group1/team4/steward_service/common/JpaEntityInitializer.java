package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import org.springframework.data.domain.Example;
import org.springframework.data.repository.query.QueryByExampleExecutor;

public interface JpaEntityInitializer<T> extends QueryByExampleExecutor<T> {
    /**
     * Find the given entity in the repository.
     * 
     * @param entity entity to find
     * @return intialized entity
     * @throws ResourceNotFoundException if entity does not exist
     */
    default T find(T entity) throws ResourceNotFoundException {
        return this.findOne(Example.of(entity)).orElseThrow(
                () -> ResourceNotFoundException.notFound(entity.getClass().getSimpleName()));
    }
}
