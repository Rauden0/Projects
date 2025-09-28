package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import org.aspectj.lang.annotation.Pointcut;

public final class Pointcuts {
    private Pointcuts() {
    }

    @Pointcut("execution(public * cz.muni.fi.pa165.spring2025.group1.team4.steward_service..*(..)) && !execution(* toString(..))")
    public static void publicMethod() {
    }
}