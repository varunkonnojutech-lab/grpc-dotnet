# gRPC in .NET 8

[![Build](https://github.com/varunkonnojutech-lab/grpc-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/varunkonnojutech-lab/grpc-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/varunkonnojutech-lab/grpc-dotnet)

A hands-on demo of [gRPC](https://grpc.io/) in .NET 8: a contract-first
`OrderService` covering **all four gRPC call types** — unary, server streaming,
client patterns, and bidirectional streaming — plus a logging interceptor and a
console client that exercises every RPC.

## Who is this for

Backend developers who know REST and want to see how service-to-service
communication works inside microservices. If you've ever wondered what replaces
REST *between* your own services, this repo answers it with running code.

## What you'll learn

- **Contract-first design**: the `.proto` file is the single source of truth; clients and servers in any language are generated from it
- **All four gRPC call types**: unary, server streaming, client streaming patterns, bidirectional streaming
- **HTTP/2 in practice**: why gRPC needs it and how Kestrel is configured for it
- **Interceptors**: cross-cutting concerns (logging, auth, retries) without touching service code

Use gRPC for internal service communication; keep REST/JSON for public APIs and browser clients.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

No other tools needed — the server runs on cleartext HTTP/2, so no TLS certificates to configure.

## Quick start

```bash
git clone https://github.com/varunkonnojutech-lab/grpc-dotnet.git
cd grpc-dotnet
```

Or click the **Codespaces** badge above for a browser-based environment with the .NET 8 SDK preinstalled.

**1. Start the server:**

```bash
dotnet run --project src/GrpcServer
```

It listens on `http://localhost:5001` with **cleartext HTTP/2** (configured in
`Program.cs` — no TLS certificate needed for local dev).

**2. Run the client (new terminal):**

```bash
dotnet run --project src/GrpcClient
```

Expected output: a unary fetch, a streamed order list, a newly placed order,
and three tracking updates (including a graceful `Unknown` for id 999).

## How it works

The contract (`protos/order.proto`) defines everything:

| RPC          | Type                   | Description                              |
| ------------ | ---------------------- | ---------------------------------------- |
| `GetOrder`   | Unary                  | Fetch one order; `NOT_FOUND` if missing  |
| `ListOrders` | Server streaming       | Stream orders, optional `status_filter`  |
| `PlaceOrder` | Unary                  | Create an order; validates input         |
| `TrackOrders`| Bidirectional streaming| Send order ids, receive live status updates |

The server references the proto with `GrpcServices="Server"`, the client with
`GrpcServices="Client"` — the C# types are generated at build time by Grpc.Tools.
`OrderGrpcService` implements each RPC against a thread-safe in-memory
`OrderStore`, returning proper `RpcException` statuses (`NotFound`,
`InvalidArgument`) instead of generic errors. The `LoggingInterceptor` hooks the
unary pipeline to log every call's start, finish, and failure.

## Project structure

```
grpc-dotnet/
├── protos/
│   └── order.proto            # Service + message contracts (the API definition)
├── src/
│   ├── GrpcServer/            # ASP.NET Core gRPC host (Grpc.AspNetCore)
│   │   ├── Services/
│   │   │   ├── OrderStore.cs        # Thread-safe in-memory order storage
│   │   │   └── OrderGrpcService.cs  # Implements all 4 RPCs
│   │   ├── Interceptors/
│   │   │   └── LoggingInterceptor.cs
│   │   └── Program.cs               # Kestrel on cleartext HTTP/2 :5001
│   └── GrpcClient/            # Console client (Grpc.Net.Client)
│       └── Program.cs               # Demos: unary, server-streaming,
│                                    # place order, bidirectional tracking
├── .devcontainer/             # One-click Codespaces setup
└── .github/workflows/ci.yml
```

## Try it with grpcurl

[grpcurl](https://github.com/fullstorydev/grpcurl) works against the cleartext
endpoint with `-plaintext`:

```bash
# Unary call
grpcurl -plaintext -d '{"id": 1}' localhost:5001 orders.OrderService/GetOrder

# Server streaming
grpcurl -plaintext -d '{"status_filter": ""}' localhost:5001 orders.OrderService/ListOrders
```

## Notes

- **HTTP/2 is mandatory** for gRPC. Browsers can't speak gRPC directly —
  use [gRPC-Web](https://github.com/grpc/grpc-dotnet/blob/master/docs/grpcweb.md)
  if a browser client is needed.
- `OrderStore` is in-memory and seeded with 3 orders; data resets on restart.
- The `LoggingInterceptor` shows how cross-cutting concerns (logging, auth,
  retries) plug into the gRPC pipeline without touching service code.

## Next steps

Continue the learning path with [azure-functions-serverless](https://github.com/varunkonnojutech-lab/azure-functions-serverless) — take event-driven ideas further with serverless functions, Service Bus messaging, and real Azure infrastructure.

## License

MIT — see [LICENSE](LICENSE).
