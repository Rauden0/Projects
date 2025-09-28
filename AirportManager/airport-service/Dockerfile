# --- Build Stage ---
FROM maven:3.9.9-eclipse-temurin-21-alpine AS build

WORKDIR /build

COPY --from=pa165-airport-manager/dependencies:2.0 /root/.m2 /root/.m2

COPY pom.xml pom.xml

COPY airport-service airport-service

RUN mvn -f airport-service/pom.xml clean package -DskipTests

# --- Runtime Stage ---
FROM eclipse-temurin:21.0.7_6-jre-jammy AS runtime

WORKDIR /app

COPY --from=build /build/airport-service/target/airport-service-*.jar app.jar

EXPOSE 8081

ENTRYPOINT ["java", "-jar", "app.jar"]
