![Order Service CI](https://github.com/nazbez/Order-Tracking-System/actions/workflows/order-service-ci.yml/badge.svg)
![Courier Service CI](https://github.com/nazbez/Order-Tracking-System/actions/workflows/courier-service-ci.yml/badge.svg)
![Core Lib CI](https://github.com/nazbez/Order-Tracking-System/actions/workflows/core-lib-ci.yml/badge.svg)
![Tracking System CI](https://github.com/nazbez/Order-Tracking-System/actions/workflows/tracking-system-ci.yml/badge.svg)

# Order-Tracking-System

![.NET](https://img.shields.io/badge/.net-9.0-green)
![Dockerized](https://img.shields.io/badge/docker-ready-brightgreen)
![License](https://img.shields.io/badge/license-MIT-yellow)

## Overview

Order-Tracking-System is a study project designed to explore and demonstrate modern software engineering practices, technologies, and architectural patterns. The system allows users to place orders and track their real-time status as they progress through various stages (e.g., "Preparing", "Out for Delivery", "Delivered").

## Project Goals

- Learn and apply new technologies and design patterns
- Build a robust, observable, and scalable distributed system
- Practice clean architecture and modern DevOps practices

## Key Features

- **Order Placement & Tracking:** Users can place orders and monitor their status in real time.
- **Real-Time Updates:** Order status is updated as it moves through different stages.
- **Microservices Architecture:** Each domain (Order, Courier, etc.) is implemented as a separate service.
- **gRPC Communication:** Services communicate using gRPC for efficient, strongly-typed inter-service messaging.
- **Clean Architecture:** Services are structured using clean architecture principles for maintainability and testability.
- **Event-Driven Communication:** Apache Kafka is used for asynchronous communication between services.
- **Outbox Pattern:** Ensures reliable event publishing from the database to Kafka.
- **Schema Registry:** Kafka messages are validated using a schema registry to ensure compatibility.
- **Observability:**
   - **Prometheus** for metrics collection
   - **Grafana** for metrics visualization
   - **Loki** for log aggregation
   - **Jaeger** for tracing
- **Persistence:** Uses both **EF Core** and **Dapper** for data access.
- **Dockerized:** All services and dependencies are containerized and orchestrated with Docker Compose.
- **CI/CD:**
   - GitHub Actions for build and test automation
   - GitHub Package Registry for publishing and consuming internal libraries

## Technologies & Tools Learned

- **Kafka** (event streaming, schema registry)
- **gRPC** (inter-service communication)
- **Entity Framework Core** & **Dapper** (data access)
- **OpenTelemetry**, **Prometheus**, **Grafana**, **Loki**, **Jaeger** (observability)
- **Docker** & **Docker Compose** (containerization & orchestration)
- **Clean Architecture** (service design)
- **Outbox Pattern** (reliable event publishing)
- **GitHub Actions** (CI/CD)
- **GitHub Package Registry** (internal package management)

## Getting Started

1. **Clone the repository:**
    ```sh
    git clone https://github.com/nazbez/Order-Tracking-System.git
    ```
2. **Provide credentials for Kafka Confluent:**
    - You must supply valid credentials for Kafka Confluent in the appropriate configuration files (e.g., `appsettings.Secrets.json` or environment variables) before starting the system.
    - Refer to the documentation or sample secrets for the required format and location.
3. **Set up environment variables:**
    - Create and configure a `.env` file with the required environment variables.
    - Run the setup script to load environment variables:
       ```sh
       ./setup-env-variables.ps1
       ```
4. **Start the system with Docker Compose:**
    ```sh
    docker-compose up --build
    ```
5. **Access services:**
    - Web APIs and dashboards will be available at the ports specified in `docker-compose.yml`.

## Project Structure

- `OrderService/` - Order management microservice
- `CourierService/` - Courier delivery microservice
- `TrackingSystem/` - Service responsible for tracking order statuses and handling real-time updates
- `Shared/` - Shared libraries and code
- `protos/` - Protocol Buffers definitions for gRPC communication between services
- `prometheus/`, `provisioning/` - Observability and monitoring configuration

## Contributing

This project is for learning and experimentation. Contributions, suggestions, and feedback are welcome!

## License

MIT License
