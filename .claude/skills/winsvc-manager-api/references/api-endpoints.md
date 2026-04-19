# Winsvc Manager API Endpoints

Base URL: see SKILL.md "Configuration" section.

All endpoints return JSON. Errors: `{"error": "message"}` with appropriate HTTP status.

## Health Check

```
GET /
```

```json
{"name": "winsvc-manager", "status": "ok"}
```

## List All Windows Services

Returns every Windows-registered service (not limited to managed ones).

```
GET /services/windows
```

```json
[
  {"id": "Dhcp", "displayName": "DHCP Client", "state": "Running", "startMode": "Auto"}
]
```

`state`: Unknown | Stopped | Starting | Running | Stopping | NotFound

`{id}` = service-id (manifest filename without `.yaml` extension).

## List Managed Services

Returns services with valid manifests only.

```
GET /services/managed
```

```json
[
  {
    "id": "<service-id>",
    "displayName": "My Application",
    "description": "...",
    "type": "managed",
    "state": "Running",
    "startMode": "delayed-auto",
    "healthUrl": "http://127.0.0.1:9000/health"
  }
]
```

## Get Service Detail

```
GET /services/{id}
```

```json
{
  "id": "<service-id>",
  "displayName": "My Application",
  "description": "...",
  "type": "managed",
  "state": "Running",
  "startMode": "delayed-auto",
  "healthUrl": "http://127.0.0.1:9000/health",
  "wrapperDir": "C:\\svc\\services\\<service-id>",
  "workDir": "C:\\svc\\runtimes\\<service-id>\\current",
  "tailscaleServeEnabled": false,
  "tailscaleServeHttpsPort": 443,
  "tailscaleServeTarget": "http://127.0.0.1:9000"
}
```

404: `{"error": "Managed service 'xxx' was not found."}`

## Health Check (per service)

```
GET /services/{id}/health
```

```json
{"id": "<service-id>", "health": "Healthy", "url": "http://127.0.0.1:9000/health", "timeoutSec": 5}
```

`health`: Unknown | Healthy | Unhealthy

## Start Service

```
POST /services/{id}/start
```

```json
{"id": "<service-id>", "action": "start", "status": "queued"}
```

## Stop Service

```
POST /services/{id}/stop
```

```json
{"id": "<service-id>", "action": "stop", "status": "queued"}
```

## Restart Service

```
POST /services/{id}/restart
```

```json
{"id": "<service-id>", "action": "restart", "status": "queued"}
```
