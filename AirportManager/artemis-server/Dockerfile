# --- Build Stage ---
FROM maven:3.9.9-eclipse-temurin-21-alpine AS build

WORKDIR /build

COPY pom.xml pom.xml

COPY artemis-server artemis-server

RUN mvn -f artemis-server/pom.xml clean package -DskipTests

# --- Runtime Stage ---
FROM eclipse-temurin:21.0.7_6-jre-jammy AS runtime

WORKDIR /app

COPY --from=build /build/artemis-server/target/artemis-server-*.jar app.jar

EXPOSE 9876

ENTRYPOINT ["java", "-jar", "-Dspring.profiles.active=docker", "app.jar"]
