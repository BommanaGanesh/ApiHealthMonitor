# ApiHealthMonitor

#  API Health Monitor

A lightweight and production-oriented ASP.NET Core Web API for monitoring the availability, HTTP status, and response time of external HTTP/HTTPS APIs.

The project demonstrates practical backend engineering concepts using C#, ASP.NET Core, asynchronous programming, dependency injection, HttpClientFactory, structured logging, automated testing, Docker, and GitHub Actions.

---

##  Overview

Modern applications depend on multiple internal and external APIs.

When one of these APIs becomes unavailable, slow, or starts returning unexpected HTTP status codes, it can affect the reliability of the entire application.

**API Health Monitor** provides a simple REST API that allows users to check the health and response performance of an HTTP/HTTPS endpoint.

For every health check, the application captures:

- API URL
- Health status
- HTTP status code
- Response time
- Check timestamp
- Error information when applicable

---

##  Features

-  HTTP/HTTPS API health checks
-  HTTP status code detection
-  Response time measurement
-  Timeout handling
-  Network failure handling
-  URL validation
-  CancellationToken support
-  Structured application logging
-  Dependency Injection
-  HttpClientFactory
-  Swagger/OpenAPI documentation
-  Unit testing
-  Docker containerization
-  GitHub Actions CI pipeline
-  SSRF security considerations

---

##  Architecture

The application follows a simple layered architecture.

```text
                    ┌─────────────────────┐
                    │       Client        │
                    │ Browser / Postman   │
                    │      Swagger        │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ HealthController    │
                    │                     │
                    │ Request validation  │
                    │ HTTP response       │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ IHealthCheckService │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ HealthCheckService  │
                    │                     │
                    │ Health check logic  │
                    │ Error handling      │
                    │ Response timing     │
                    │ Logging             │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │    HttpClient       │
                    │   HttpClientFactory │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │   External API      │
                    └─────────────────────┘
