package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import lombok.AccessLevel;
import lombok.Getter;
import lombok.RequiredArgsConstructor;

import java.util.Collection;
import java.util.HashSet;
import java.util.function.Consumer;

@RequiredArgsConstructor
public abstract class JmsEventDispatcher<E> {
    @Getter(value = AccessLevel.PROTECTED)
    private final Collection<Consumer<E>> listeners = new HashSet<>();

    public void addEventListener(Consumer<E> listener) {
        listeners.add(listener);
    }

    protected void broadcastEvent(E event) {
        for (var listener : listeners) {
            listener.accept(event);
        }
    }
}
