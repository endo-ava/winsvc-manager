---
name: winsvc-manager-api
description: >
  Use the winsvc-manager REST API to list Windows services, view managed service details, check service health, and start/stop/restart managed services. Trigger when the user wants toremotely control Windows services, check service status, inspect managed service configuration, or interact with the winsvc-manager API. Assumes manifests are already provisioned.
---

# Winsvc Manager API

REST API for remote Windows service management via winsvc-manager.

## Configuration

Set `WINSVC_URL` before calling any endpoint. AI must write the `export` line using the URL defined here.

<!-- USER: Replace the URL below with your actual endpoint -->
**Endpoint**: `http://127.0.0.1:8011`
<!-- Tailscale Serve example: https://<machine-name>.<tailnet>.ts.net -->

AI usage pattern:
```bash
export WINSVC_URL="http://127.0.0.1:8011"   # copied from above
curl "$WINSVC_URL/"
```

## Quick Reference

| Action | Method | Path |
|---|---|---|
| API health check | GET | `/` |
| List all Windows services | GET | `/services/windows` |
| List managed services | GET | `/services/managed` |
| Get service detail | GET | `/services/{id}` |
| Check service health | GET | `/services/{id}/health` |
| Start service | POST | `/services/{id}/start` |
| Stop service | POST | `/services/{id}/stop` |
| Restart service | POST | `/services/{id}/restart` |

`{id}` in paths refers to the **service-id** — the manifest filename without extension (e.g. `my-app.yaml` → service-id is `my-app`).

## Common Workflows

### Check if API is reachable

```bash
curl "$WINSVC_URL/"
# => {"name":"winsvc-manager","status":"ok"}
```

### View all running Windows services

```bash
curl "$WINSVC_URL/services/windows"
```

### See managed services and their states

```bash
curl "$WINSVC_URL/services/managed"
```

### Get detailed info for a specific service

```bash
curl "$WINSVC_URL/services/{id}"
# e.g. curl "$WINSVC_URL/services/my-app"
```

Response includes: paths (`wrapperDir`, `workDir`), state, Tailscale Serve settings.

### Check if a service is healthy

```bash
curl "$WINSVC_URL/services/{id}/health"
# => {"id":"my-app","health":"Healthy","url":"http://127.0.0.1:9000/health","timeoutSec":5}
```

`health` values: `Unknown` | `Healthy` | `Unhealthy`

### Start / Stop / Restart a service

All return `{"status":"queued"}` immediately (fire-and-forget).

```bash
curl -X POST "$WINSVC_URL/services/{id}/start"
curl -X POST "$WINSVC_URL/services/{id}/stop"
curl -X POST "$WINSVC_URL/services/{id}/restart"
```

## Notes

- **No authentication** is implemented. Security relies on network isolation (localhost binding or Tailscale ACLs).
- **Install/Uninstall** are CLI-only operations, not exposed via API.
- Service `state` values: `Unknown` | `Stopped` | `Starting` | `Running` | `Stopping` | `NotFound`
- Services with invalid manifests are silently excluded from `/services/managed` and `/services/{id}`.

## Full Endpoint Reference

See [references/api-endpoints.md](references/api-endpoints.md) for complete request/response schemas.
