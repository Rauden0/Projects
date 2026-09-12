package httpserver_test

import (
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

func TestWriteJSON_SetsHeadersAndEncodesBody(t *testing.T) {
	rec := httptest.NewRecorder()

	httpserver.WriteJSON(rec, http.StatusCreated, map[string]string{"hello": "world"})

	assert.Equal(t, http.StatusCreated, rec.Code)
	assert.Equal(t, "application/json; charset=utf-8", rec.Header().Get("Content-Type"))
	assert.Equal(t, "no-store", rec.Header().Get("Cache-Control"))

	var got map[string]string
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&got))
	assert.Equal(t, "world", got["hello"])
}

func TestWriteJSON_NilBodyWritesNoContent(t *testing.T) {
	rec := httptest.NewRecorder()

	httpserver.WriteJSON(rec, http.StatusNoContent, nil)

	assert.Equal(t, http.StatusNoContent, rec.Code)
	assert.Empty(t, rec.Body.Bytes())
}

func TestWriteError_MapsDomainErrorsToStatusAndCode(t *testing.T) {
	cases := []struct {
		name       string
		err        error
		wantStatus int
		wantCode   string
	}{
		{"not found", domain.ErrNotFound, http.StatusNotFound, "not_found"},
		{"already exists", domain.ErrAlreadyExists, http.StatusConflict, "already_exists"},
		{"invalid argument", domain.ErrInvalidArgument, http.StatusBadRequest, "invalid_argument"},
		{"self tracking", domain.ErrSelfTracking, http.StatusBadRequest, "self_tracking"},
		{"unauthenticated", domain.ErrUnauthenticated, http.StatusUnauthorized, "unauthenticated"},
		{"payload too large", domain.ErrPayloadTooLarge, http.StatusRequestEntityTooLarge, "payload_too_large"},
		{"unrecognized error", errors.New("boom"), http.StatusInternalServerError, "internal_error"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			rec := httptest.NewRecorder()
			httpserver.WriteError(rec, tc.err)

			require.Equal(t, tc.wantStatus, rec.Code)

			var body struct {
				Error struct {
					Code    string `json:"code"`
					Message string `json:"message"`
				} `json:"error"`
			}
			require.NoError(t, json.NewDecoder(rec.Body).Decode(&body))
			assert.Equal(t, tc.wantCode, body.Error.Code)
		})
	}
}

func TestWriteError_WrappedErrorPreservesMessageBelow500(t *testing.T) {
	rec := httptest.NewRecorder()
	err := fmt.Errorf("%w: latitude must be between -90 and 90", domain.ErrInvalidArgument)
	httpserver.WriteError(rec, err)

	var body struct {
		Error struct {
			Message string `json:"message"`
		} `json:"error"`
	}
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&body))
	assert.Contains(t, body.Error.Message, "latitude must be between -90 and 90")
}

func TestWriteError_MasksMessageAt500AndAbove(t *testing.T) {
	rec := httptest.NewRecorder()
	httpserver.WriteError(rec, errors.New("pq: connection reset by peer, password=hunter2"))

	var body struct {
		Error struct {
			Message string `json:"message"`
		} `json:"error"`
	}
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&body))
	assert.Equal(t, "internal server error", body.Error.Message)
	assert.NotContains(t, body.Error.Message, "hunter2")
}
