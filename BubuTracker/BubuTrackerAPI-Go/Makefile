.PHONY: build test vet fmt lint sqlc-generate migrate-up migrate-down compose-up compose-down

build:
	go build ./...

test:
	go test ./... -cover

vet:
	go vet ./...

fmt:
	gofmt -l .

sqlc-generate:
	docker run --rm -v "$(CURDIR):/src" -w /src/db sqlc/sqlc generate

migrate-up:
	go run ./cmd/migrate -direction up

migrate-down:
	go run ./cmd/migrate -direction down

compose-up:
	docker compose -f deploy/docker-compose.yml --env-file .env up --build

compose-down:
	docker compose -f deploy/docker-compose.yml down -v
