# T1 Knative Administrator

A three-day hackathon project from *T1 holding: *FaaS Billing System**

A system for monitoring and managing Knative functions with real-time billing and metrics.

## Functionality

- **Metrics Monitoring**: Collection and display of CPU, memory, request count, and execution time
- **Real-time Billing**: Calculation of function execution costs based on resource usage
- **Function Management**: Launch functions with customizable parameters
- **Knative Integration**: Automatic metrics collection from Knative Serving
- **Prometheus Monitoring**: Integration with Prometheus for metrics collection
- **Web Interface**: Dashboard with feature testing capability, plus OpenApi | SwaggerUI support (/swagger/index.html)

## Technology Stack

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, Kubernetes Client
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **Database**: In-Memory Database (Abstracted by EF Core, allows easy integration with any DBMS)
- **Monitoring**: Prometheus
- **Orchestration**: Kubernetes, Knative Serving
- **Containerization**: Docker

## Quick Start

### Prerequisites

- Kubernetes cluster with Knative and kube-prometheus-stack
- Docker

### Installation

1. **Clone the repository**

```
git clone https://github.com/NikitaBurinsky/t1-knative-monitor-v0.1.git
```

2. Build and run

You need to build the Docker image and run it, exposing the local port where the Kubernetes cluster is running. The simplest way to do this is using the `--host` flag:

```
docker build -t <name> .
docker run --host <name>:latest
```

3. Configure Prometheus

```
kubectl apply -f service-monitor.yaml
```

4. Access the application

- Web interface: [http://localhost:8080/index.html](http://localhost:8080/index.html)
- Swagger UI: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

## Configuration and Pricing Plans

Application configuration description is available in [CONFIGURATION.md](CONFIGURATION.md). All settings are located in the [appsettings.json](./appsettings.json) file in the repository root.

## Architecture

The architecture is described in [ARCHITECTURE.md](ARCHITECTURE.md).
