## Architecture.md

# Архитектура T1 Knative Administrator v03

## Обзор системы

Система представляет собой микросервисную архитектуру для мониторинга, управления и биллинга Knative-функций. Архитектура построена на принципах Domain-Driven Design (DDD) и разделена на четкие слои ответственности.

## Высокоуровневая архитектура
    ┌─────────────────┐ ┌──────────────────┐ ┌─────────────────┐
    │ Веб-клиент     │◄──►│ ASP.NET Core  │◄──►│ Kubernetes    │
    │ (Dashboard)     │ │ API (5151)       │ │ Cluster         │
    └─────────────────┘ └──────────────────┘ └─────────────────┘
    │ │ │                     │
    │ │ └───► Prometheus ◄────┘
    │ │ (9090)
    │ │
    └───◄ Static Files ─────┘ │
    (HTML/CSS/JS) ▼
    ┌──────────────────┐
    │ Knative Control  │
    │ Plane Metrics    │
    └──────────────────┘

## Компонентная диаграмма

### Слои архитектуры

#### 1. **Presentation Layer**
- **FunctionRunnerController**: Основной контроллер для управления функциями
  - `GET /runner/echo/get-metrics` - получение метрик
  - `POST /runner/echo` - запуск функции (заглушка)
- **Static Files**: Веб-интерфейс с дашбордом
- **Swagger UI**: Автоматическая документация API

#### 2. **Application Layer**
- **Services**: 
  - `FunctionsStatsManagerService` - оркестрация управления метриками
  - `KnativeControlMetricsCollector` - сбор метрик из Knative/Prometheus
- **Background Services**: Периодический сбор метрик (30 сек интервал)

#### 3. **Domain Layer**
- **Entities**: 
  - `FunctionEntity` - центральная сущность системы
- **Value Objects**: 
  - `RunningTimeStats`, `VCPUTimeStats`, `RequestsCounterStats`, `RAMStats`
- **OpResult Pattern**: Унифицированная обработка результатов операций
- **Prometheus Models**: Модели для работы с Prometheus API

#### 4. **Infrastructure Layer**
- **Repositories**: 
  - `FunctionsInfoRepository` - CRUD операции для функций
  - `ApplicationDbContext` - контекст Entity Framework
- **External Services**: 
  - Kubernetes Client API
  - Prometheus HTTP API
- **Writer Profiles**: Система обработки различных типов метрик

## Детальные потоки данных

### 1. Сбор метрик (каждые 30 секунд)
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
ApplicationDbContext (In-Memory)

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
```
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

    - RunningTimeStats: Время выполнения функции (макс, среднее, счетчик)

    - VCPUTimeStats: Использование CPU (аналогично времени выполнения)

    - RequestsCounterStats: Количество запросов с группировкой по дням

    - RAMStats: Использование памяти в байтах

    - OpResult: унифицированная обработка операций
```
// Базовый результат операции
public record OpResult(bool Succeeded, HttpStatusCode? ErrCode, string? Message)

// Generic версия с возвращаемыми данными  
public record OpResult<TData> : OpResult
{
    public TData? Returns { get; }
}
```
**Преимущества:**

    - Единообразная обработка успехов/ошибок

    - Интеграция с ASP.NET Core ProblemDetails

    - Поддержка HTTP статус кодов

    - Type-safe generic версии

### Система Writer Profiles
**Базовый абстрактный класс:**
```
public abstract class BaseStatsWriterProfile
{
    public abstract OpResult WriteStatsMetric(FunctionEntity functionData, 
                                            PrometheusData metricData, 
                                            string query);
}
```
**Реализация RAMStatsProfile:**
```
public class RAMStatsProfile : BaseStatsWriterProfile
{
    public override OpResult WriteStatsMetric(FunctionEntity functionData, 
                                            PrometheusData metricData, 
                                            string query)
    {
        // Обработка container_memory_usage_bytes
        // Расчет максимального и среднего значения
        // Инкрементальное обновление статистики
    }
}
```
**Принцип работы:**

    - Маршрутизация по паттерну query string

    - Типизированная обработка специфичных метрик

    - Инкрементальное обновление статистики

    - Поддержка скользящих средних

## Prometheus интеграция
### Модели данных:
```
public class PrometheusQueryResponse
{
    public string Status { get; set; }           // "success" | "error"
    public PrometheusData Data { get; set; }     // Основные данные
}

public class PrometheusData
{
    public string ResultType { get; set; }       // "matrix" | "vector"
    public List<PrometheusResult> Result { get; set; }
}

public class PrometheusResult
{
    public Dictionary<string, string> Metric { get; set; }  // Метки
    public List<(long Timestamp, string Value)> Values { get; set; } // Временные ряды
}
```
### Система запросов
```
// Автоматическая генерация query range запросов
public async Task<string> GetBodyStringAsync(string query, int step = 60, 
    List<(string param, string value)> addictionalParameters = null)
{
    // Динамическое построение URL с временными диапазонами
    // Поддержка дополнительных параметров
    // Обработка временных меток
}
```

## Frontend Architecture
### Компонентная структура:
```
index.html
├── Header (переключатель темы)
├── Metrics Section (таблица метрик)
├── Runner Section (форма запуска)
├── Billing Section (расчет стоимости)
└── Easter Egg (Spamton GIF)
JavaScript модули
javascript
// API Client
const API_BASE = "http://localhost:5151/runner/echo";

// Core Functions
async function loadMetrics()          // Загрузка и рендеринг метрик
function renderMetrics(data)          // Обновление UI таблицы
function calculateBilling(data)       // Расчет и отображение биллинга

// Event Handlers
document.getElementById("theme-toggle")  // Переключение темы
document.getElementById("runner-form")   // Отправка формы
document.querySelector("h1")             // Easter egg
Биллинг алгоритм
javascript
// Константы расчета
const CPU_RATE = 0.00005;
const RAM_RATE = 0.00001; 
const REQ_RATE = 0.001;

// Формулы
cpuCost = avgCpu × avgRunTime × CPU_RATE
ramCost = avgRam(MB) × avgRunTime × RAM_RATE
reqCost = requests × REQ_RATE
total = cpuCost + ramCost + reqCost
⚙️ Конфигурационная система
Многоуровневая конфигурация
csharp
// appsettings.json - базовые настройки
// appsettings.Development.json - dev настройки
// Environment Variables - продуктион настройки
// Kubernetes ConfigMaps/Secrets - оркестрация
```
### Основные секции:
- BillingConfigurations: Тарифы и цены

- Seeding: Начальные данные для тестирования

- Logging: Настройки логирования

- Kubernetes: Конфигурация кластера

## Жизненный цикл приложения
**Startup последовательность (Program.cs)**
```
    1. WebApplication.CreateBuilder()
    2. Services Configuration (DbContext, Repositories, Services)
    3. Kubernetes Client Setup (in-cluster/local config)
    4. Database Seeding (тестовые данные)
    5. Background Service Start (сбор метрик)
    6. API Endpoints Mapping
```
**Фоновые задачи**
```
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await knativeControlCollector.CollectAsync();
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        });
    });
```
## Безопасность
**Текущая реализация**
    - In-cluster Kubernetes аутентификация

    - Prometheus без аутентификации (dev среда)

    - HTTP API (для внутреннего использования)

**Production рекомендации**
    - HTTPS/TLS для всех endpoints

    - Prometheus с Basic Auth/TLS

    - Kubernetes Service Accounts с ограниченными правами

    - Валидация входных параметров

    - Rate limiting для API

## Масштабируемость
**Горизонтальное масштабирование**
    - Stateless API: Может запускаться в нескольких репликах

    - In-Memory DB: Может быть заменен на Redis/SQL Server

    - Фоновые задачи: Могут быть вынесены в отдельные Pod'ы

**Оптимизации производительности**
    - Асинхронная обработка во всех слоях

    - Пакетные обновления метрик

    - Эффективные запросы к Prometheus (range queries)

    - Кэширование частых запросов

## Архитектурные решения
1. Owned Types в EF Core
    - Использование [Owned] атрибута для композитных типов метрик позволяет:

    - Сохранять сложные объекты в одной таблице

    - Упрощать запросы к данным

    - Поддерживать целостность данных

2. Generic OpResult Pattern
    - Унифицированная обработка результатов обеспечивает:

    - Единообразный error handling

    - Легкую интеграцию с ASP.NET Core

    - Type-safe возвращаемые значения

3. Writer Profiles System
    - Плагинная архитектура для обработки метрик позволяет:

    - Легко добавлять новые типы метрик

    - Изолировать логику обработки

    - Тестировать компоненты независимо

4. Background Metrics Collection
    - Периодический сбор метрик обеспечивает:

    - Актуальность данных в реальном времени

    - Отказоустойчивость (продолжение работы при временных ошибках)

    - Прогнозируемую нагрузку на Prometheus

Эта архитектура обеспечивает надежную основу для мониторинга Knative-функций с возможностью легкого расширения и поддержки.