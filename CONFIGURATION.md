# CONFIGURATION.md

## Configuration and Functionality Extension Overview

This configuration file contains settings for the serverless functions monitoring and billing system. It also provides additional information for adding new metrics and statistics.

## Adding Metrics

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

**How to Add:**
- Add new Prometheus query to the corresponding service in `KnativeControlMetricsCollector` (if new information from Prometheus is required)
- Add new required business fields to `FunctionEntity`
- Create a profile class inheriting from `BaseStatsWriterProfile` that converts query results to corresponding metrics and updates information in the respective `FunctionEntity`
- Register the profile in `FunctionStatsManagerService.WriteMetrics` with corresponding execution conditions

## Configuration Sections

### 1. Logging
Application logging settings.

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

**Parameters:**
- `Default` - default logging level: `Information`
- `Microsoft.AspNetCore` - logging level for ASP.NET Core: `Warning`

### 2. BillingConfigurations
Billing settings for calculating function execution costs.

```json
"BillingConfigurations": {
  "Base": {
    "PricePerRequest": 0.0000002,
    "PricePerGBSeconds": 0.00001667,
    "PricePerVCpuSecond": 0.0405,
    "PricePerGBMemorySecond": 0.000001231
  }
}
```

**Parameters:**
- `PricePerRequest` - cost per request
- `PricePerGBSeconds` - cost per gigabyte-second
- `PricePerVCpuSecond` - cost per vCPU second
- `PricePerGBMemorySecond` - cost per GB memory second

### 3. FunctionCostSettings
Alternative/additional function cost calculation system.

```json
"FunctionCostSettings": {
  "BaseCostPerDay": 0.10,
  "CostPerRequest": 0.0001,
  "CostPerMsCpu": 0.000001,
  "CostPerMsRuntime": 0.0000005,
  "CostPerMbRam": 0.000002,
  "CostPerMbRamHour": 0.0001,
  "Currency": "USD"
}
```

**Parameters:**
- `BaseCostPerDay` - base cost per day
- `CostPerRequest` - cost per request
- `CostPerMsCpu` - cost per CPU millisecond
- `CostPerMsRuntime` - cost per runtime millisecond
- `CostPerMbRam` - cost per megabyte of memory
- `CostPerMbRamHour` - cost per megabyte-hour of memory
- `Currency` - calculation currency

### 4. Seeding
Settings for function metrics initialization and collection.

```json
"Seeding": {
  "FunctionsInfo": {
    "ServingName": "echo-00001",
    "RevisionName": "deployment",
    "PODName": "5f657c6b6b-dkmhk",
    "FunctionUrl": "http://127.0.0.1:8081/api/v1/metrics?param=value"
  }
}
```

**Parameters:**
- `ServingName` - echo function name
- `RevisionName` - revision name
- `PODName` - POD name in Kubernetes
- `FunctionUrl` - URL for accessing the echo function

### 5. Collectors
Metrics collectors settings.

```json
"Collectors": {
  "Prometheus": {
    "CollectingDelaysSeconds": 300
  }
}
```

**Parameters:**
- `CollectingDelaysSeconds` - delay between metrics collections in seconds: 300 (5 minutes)

### 6. AllowedHosts
Host security settings.

```json
"AllowedHosts": "*"
```

**Value:**
- `*` - all hosts allowed
