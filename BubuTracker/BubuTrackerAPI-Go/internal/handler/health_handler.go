package handler

import (
	"context"
	"net/http"
	"time"

	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

// Pinger is satisfied by *pgxpool.Pool; kept as a narrow interface so this
// handler doesn't need to import pgx.
type Pinger interface {
	Ping(ctx context.Context) error
}

type HealthHandler struct {
	db Pinger
}

func NewHealthHandler(db Pinger) *HealthHandler {
	return &HealthHandler{db: db}
}

// Live reports liveness unconditionally: if the process can respond at all,
// it's alive. Used by orchestrators to decide whether to restart the pod.
func (h *HealthHandler) Live(w http.ResponseWriter, r *http.Request) {
	httpserver.WriteJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}

// Ready additionally checks the database is reachable. Used by orchestrators
// to decide whether to route traffic to this instance.
func (h *HealthHandler) Ready(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 2*time.Second)
	defer cancel()

	if err := h.db.Ping(ctx); err != nil {
		httpserver.WriteJSON(w, http.StatusServiceUnavailable, map[string]string{"status": "unavailable"})
		return
	}
	httpserver.WriteJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}
