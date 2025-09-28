package cz.muni.fi.pa165.spring2025.group1.team4.steward_service.common;

import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.Signature;
import org.aspectj.lang.annotation.Around;
import org.aspectj.lang.annotation.Aspect;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

import java.util.Arrays;

@Profile("trace")
@Aspect
@Component
public class LoggingAspect {

    private final Logger logger = LoggerFactory.getLogger(this.getClass());

    @Around("Pointcuts.publicMethod()")
    public Object logAround(ProceedingJoinPoint joinPoint) throws Throwable {
        Signature method = joinPoint.getSignature();

        String arguments = Arrays.toString(joinPoint.getArgs());

        arguments = arguments.substring(1, arguments.length() - 1);

        logger.trace("{}.{}({})",
                method.getDeclaringType().getSimpleName(),
                method.getName(), arguments);

        Object result = joinPoint.proceed();

        logger.trace("{}.{}(...) => {}",
                method.getDeclaringType().getSimpleName(),
                method.getName(), result);

        return result;
    }
}
