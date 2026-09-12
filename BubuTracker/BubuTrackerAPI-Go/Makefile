.PHONY: build test test-integration vet fmt lint sqlc-generate migrate-up migrate-down compose-up compose-down

build:
	go build ./...

test:
	go test ./... -cover

# Runs the internal/store suite against a real, disposable Postgres — the
# layer fakes can't verify (see internal/store/integration_helpers_test.go).
# Needs a Go toolchain and Docker locally; CI runs the same tests against a
# Postgres service container instead of docker-compose.
test-integration:
	docker compose -f deploy/docker-compose.yml up -d postgres
	@echo "waiting for postgres..."
	@until docker compose -f deploy/docker-compose.yml exec -T postgres pg_isready -U bubutracker >/dev/null 2>&1; do sleep 1; done
	DATABASE_URL="postgres://bubutracker:bubutracker@localhost:5432/bubutracker?sslmode=disable" \
		go run ./cmd/migrate -direction up
	TEST_DATABASE_URL="postgres://bubutracker:bubutracker@localhost:5432/bubutracker?sslmode=disable" \
		go test -tags=integration ./internal/store/... -v; \
		status=$$?; \
		docker compose -f deploy/docker-compose.yml down -v; \
		exit $$status

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
