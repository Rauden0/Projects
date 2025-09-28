.PHONY = prod dev

prod:
	mvn clean spring-boot:run

dev:
	mvn clean spring-boot:run -Dspring-boot.run.profiles=dev

trace:
	mvn clean spring-boot:run -Dspring-boot.run.profiles=trace
