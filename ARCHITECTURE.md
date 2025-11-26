# T1 Knative Administrator Architecture

## System Overview

The system provides an architecture for monitoring, managing, and billing Knative functions. The architecture is built on Domain-Driven Design (DDD) principles and divided into clear responsibility layers.

## Architecture
<img width="1985" height="838" alt="image" src="https://github.com/user-attachments/assets/5e1d9c6f-d935-418b-8a63-c21d91124647" />
Architecture diagram file: `https://drive.google.com/file/d/1CbvkmlrjqTSH_ZPB5KtUMIlmd11eDgoX/view?usp=sharing`

### Architecture Layers

#### 1. **Presentation Layer**

- **FunctionRunnerController**: Main controller for function management
  - `GET /runner/echo/get-metrics` - retrieve metrics
  - `POST /runner/echo` - start function
  - `POST /billings/start-period` - start function usage period
  - `POST /runner/echo` - complete, calculate, and bill for function usage period
- **Web UI**:
  - `/index.html` Web interface
  - `/swagger/index.html` SwaggerUI web interface
- **MVC** - APIs implemented using ASP.NET Core WebApi controllers

#### 2. **Application Layer**

- **Services**:
  - `FunctionsStatsManagerService` - orchestrates metrics management
  - `KnativeControlMetricsCollector` - collects metrics from Knative/Prometheus
- **Background Services**: Periodic metrics collection

#### 3. **Domain Layer**

- **Entities**:
  - `FunctionEntity` - central system entity
- **Value Objects**:
  - `RunningTimeStats`, `VCPUTimeStats`, `RequestsCounterStats`, `RAMStats`
- **OpResult Pattern**: Unified operation result handling to minimize the impact of interruptions on system speed
- **Prometheus Models**: Models for working with Prometheus API

#### 4. **Infrastructure Layer**

- **Repositories**:
  - `FunctionsInfoRepository` - CRUD operations for functions
  - `ApplicationDbContext` - Entity Framework context
- **External Services**:
  - Kubernetes Client API
  - Prometheus HTTP API
- **Writer Profiles**: System for processing different types of metrics and extracting required information from them. New profiles can be added to extend the list of metrics and statistics

## Detailed Data Flows

### 1. Metrics Collection

Knative Components\
↓
Prometheus (9090) ——————→ Metrics Queries\
↓
KnativeControlMetricsCollector.CollectAsync()\
↓
FunctionsStatsManagerService.WriteMetrics()\
↓
RAMStatsProfile.WriteStatsMetric() —→ Other Profiles\
↓
FunctionsInfoRepository.UpdateFunctionInfo()\
↓
ApplicationDbContext (In-Memory, EF Core, with potential easy integration of all popular DBMS)

### 2. Metrics Request via API

Web Client → GET /runner/echo/get-metrics\
↓
FunctionRunnerController.GetMetrics()\
↓
FunctionsInfoRepository.Get("echo-00001-deployment-...")\
↓
FunctionEntity (with metrics) → JSON Response\
↓
Web Dashboard (table rendering + billing)

### 3. Prometheus Data Processing

Prometheus HTTP Response → JSON\
↓
PrometheusQueryResponse (deserialization)\
↓
PrometheusData.Result[] → List<PrometheusResult>\
↓
Metric.Values → List<(Timestamp, Value)>\
↓
Writer Profiles (typed processing)

## Domain Models

### FunctionEntity - System Core
```csharp
public class FunctionEntity
{
    public int Id { get; set; }
    public string FullName { get; set; }       // "serving-revision-pod"
    public string RevisionName { get; set; }   // "deployment"
    public string ServingName { get; set; }    // "echo-00001"  
    public string PODName { get; set; }        // "5f657c6b6b-dkmhk"

    // Composite metrics (Owned Types in EF Core)
    public RunningTimeStats RunTimeStats { get; set; }
    public VCPUTimeStats vCpuStats { get; set; }
    public RequestsCounterStats requestsCounterStats { get; set; }
    public RAMStats RamStats { get; set; }
}
```

### Nested Metric Classes:

- `RunningTimeStats`: Function execution time (max, average, count)
- `VCPUTimeStats`: CPU usage (similar to execution time)
- `RequestsCounterStats`: Request count grouped by days
- `RAMStats`: Memory usage in bytes

### Writer Profiles System

**Base abstract class:**

```csharp
public abstract class BaseStatsWriterProfile
{
    public abstract OpResult WriteStatsMetric(FunctionEntity functionData, 
                                            PrometheusData metricData, 
                                            string query);
}
```

**Operation Principle:**

- Routing based on query string patterns
- Typed processing of specific metrics
- Incremental statistics updates
- Support for moving averages
- Encapsulation of metric extraction logic from pure data
