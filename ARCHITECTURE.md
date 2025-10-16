# Архитектура T1 Knative Administrator

## Обзор системы

Система представляет собой микросервисную архитектуру для мониторинга, управления и биллинга Knative-функций. Архитектура построена на принципах Domain-Driven Design (DDD) и разделена на четкие слои ответственности.

## Высокоуровневая архитектура
<img width="1985" height="838" alt="image" src="https://github.com/user-attachments/assets/5e1d9c6f-d935-418b-8a63-c21d91124647" />

### Слои архитектуры

#### 1. **Presentation Layer**

- **FunctionRunnerController**: Основной контроллер для управления функциями
  - `GET /runner/echo/get-metrics` - получение метрик
  - `POST /runner/echo` - запуск функции
  - `POST /billings/start-period` - запуск периода использования функции
  - `POST /runner/echo` - завершение, подсчет и выставление счета за период использования функции
- **Web UI**: Веб-интерфейс с дашбордом
- **MVC** - API реализованы контроллерами WebApi ASP.NET Core

#### 2. **Application Layer**

- **Services**: 
  - `FunctionsStatsManagerService` - оркестрация управления метриками
  - `KnativeControlMetricsCollector` - сбор метрик из Knative/Prometheus
- **Background Services**: Периодический сбор метрик

#### 3. **Domain Layer**

- **Entities**: 
  - `FunctionEntity` - центральная сущность системы
- **Value Objects**: 
  - `RunningTimeStats`, `VCPUTimeStats`, `RequestsCounterStats`, `RAMStats`
- **OpResult Pattern**: Унифицированная обработка результатов операций, для минимизации влияния прерываний на скорость системы
- **Prometheus Models**: Модели для работы с Prometheus API

#### 4. **Infrastructure Layer**

- **Repositories**: 
  - `FunctionsInfoRepository` - CRUD операции для функций
  - `ApplicationDbContext` - контекст Entity Framework
- **External Services**: 
  - Kubernetes Client API
  - Prometheus HTTP API
- **Writer Profiles**: Система обработки различных типов метрик и выделение из них требуемой информации. Могут быть добавлены новые профили, для расширения списка метрик и статов

## Детальные потоки данных

### 1. Сбор метрик

Knative Components\
↓
Prometheus (9090) ——————→ Metrics Queries\
↓
KnativeControlMetricsCollector.CollectAsync()\
↓
FunctionsStatsManagerService.WriteMetrics()\
↓
RAMStatsProfile.WriteStatsMetric() —→ Другие Profiles\
↓
FunctionsInfoRepository.UpdateFunctionInfo()\
↓
ApplicationDbContext (In-Memory, EF Core, возможная легкая интеграция всех популярных СУБД)

### 2. Запрос метрик через API

Web Client → GET /runner/echo/get-metrics\
↓
FunctionRunnerController.GetMetrics()\
↓
FunctionsInfoRepository.Get("echo-00001-deployment-...")\
↓
FunctionEntity (с метриками) → JSON Response\
↓
Web Dashboard (рендеринг таблицы + биллинг)

### 3. Обработка Prometheus данных

Prometheus HTTP Response → JSON\
↓
PrometheusQueryResponse (десериализация)\
↓
PrometheusData.Result[] → List<PrometheusResult>\
↓
Metric.Values → List<(Timestamp, Value)>\
↓
Writer Profiles (типизированная обработка)

## Доменные модели

### FunctionEntity - ядро системы
```csharp
public class FunctionEntity
{
    public int Id { get; set; }
    public string FullName { get; set; }       // "serving-revision-pod"
    public string RevisionName { get; set; }   // "deployment"
    public string ServingName { get; set; }    // "echo-00001"  
    public string PODName { get; set; }        // "5f657c6b6b-dkmhk"

    // Композитные метрики (Owned Types в EF Core)
    public RunningTimeStats RunTimeStats { get; set; }
    public VCPUTimeStats vCpuStats { get; set; }
    public RequestsCounterStats requestsCounterStats { get; set; }
    public RAMStats RamStats { get; set; }
}
```

### Вложенные классы метрик:

- `RunningTimeStats`: Время выполнения функции (макс, среднее, счетчик)
- `VCPUTimeStats`: Использование CPU (аналогично времени выполнения)
- `RequestsCounterStats`: Количество запросов с группировкой по дням
- `RAMStats`: Использование памяти в байтах

### Система Writer Profiles

**Базовый абстрактный класс:**

```csharp
public abstract class BaseStatsWriterProfile
{
    public abstract OpResult WriteStatsMetric(FunctionEntity functionData, 
                                            PrometheusData metricData, 
                                            string query);
}
```

**Принцип работы:**

- Маршрутизация по паттерну query string
- Типизированная обработка специфичных метрик
- Инкрементальное обновление статистики
- Поддержка скользящих средних
- Инкапсуляция логики выделения метрик из pure data 
