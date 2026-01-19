# dotnet-websocket-gateway

A lightweight .NET background service that connects to an external WebSocket,
receives messages, and forwards them to a configurable HTTP webhook.
It also exposes an HTTP API that accepts requests and sends messages to the connected WebSocket.

## Features
- BackgroundService-based WebSocket client
- Automatic reconnect handling
- Asynchronous message forwarding
- Clean separation of concerns
- Configuration-driven endpoints

## Architecture
External WebSocket → BackgroundService → Event → WebhookSender  
HTTP API → BackgroundService → External WebSocket

## Tech Stack
- .NET 9
- ASP.NET Core
- ClientWebSocket
- HttpClientFactory

## Usage
1. Configure the external WebSocket URL in `appsettings.json`
2. Configure the webhook endpoint URL
3. Run the application

## Use Cases
- Realtime event gateways
- Protocol translation (WebSocket → HTTP)
- Integration layers between realtime systems and REST-based backends
