# CONFIGURATION.md

## Обзор конфигурации и расширения функционала

Этот файл конфигурации содержит настройки для системы мониторинга и биллинга функций (serverless functions).
Также приводятся дополнительные сведения для добавления новых метрик и статов

## Добавление метрик

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

**Способ добавления**
- Добавление нового Prometheus query к соответсвующему сервису в `KnativeControlMetricsCollector` (если требуется новая информация от Prometheus)
- Добавление новых искомых бизнес-полей в `FunctionEntity`
- Создание класса профиля, наследника `BaseStatsWriterProfile`, приводящего результат запроса к соответсвующим метрикам и изменяющий информацию в соответсвующум `FunctionEntity`
- Регистрация профиля в `FunctionStatsManagerService.WriteMetrics` с соответвующими условиями запуска  

## Секции конфигурации

### 1. Logging
Настройки логирования приложения.

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

**Параметры:**
- `Default` - уровень логирования по умолчанию: `Information`
- `Microsoft.AspNetCore` - уровень логирования для ASP.NET Core: `Warning`

### 2. BillingConfigurations
Настройки тарификации для вычисления стоимости выполнения функций.

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

**Параметры:**
- `PricePerRequest` - стоимость одного запроса
- `PricePerGBSeconds` - стоимость за гигабайт-секунду
- `PricePerVCpuSecond` - стоимость за секунду использования vCPU
- `PricePerGBMemorySecond` - стоимость за секунду использования памяти

### 3. FunctionCostSettings
Альтернативная/дополнительная система расчета стоимости функций.

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

**Параметры:**
- `BaseCostPerDay` - базовая стоимость в день
- `CostPerRequest` - стоимость за запрос
- `CostPerMsCpu` - стоимость за миллисекунду CPU
- `CostPerMsRuntime` - стоимость за миллисекунду времени выполнения
- `CostPerMbRam` - стоимость за мегабайт памяти
- `CostPerMbRamHour` - стоимость за мегабайт-час памяти
- `Currency` - валюта расчетов

### 4. Seeding
Настройки для инициализации и сбора метрик функций.

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

**Параметры:**
- `ServingName` - имя echo функции
- `RevisionName` - имя ревизии
- `PODName` - имя POD в Kubernetes
- `FunctionUrl` - URL для обращения к функции echo

### 5. Collectors
Настройки коллекторов метрик.

```json
"Collectors": {
  "Prometheus": {
    "CollectingDelaysSeconds": 300
  }
}
```

**Параметры:**
- `CollectingDelaysSeconds` - задержка между сборами метрик в секундах: 300 (5 минут)

### 6. AllowedHosts
Настройки безопасности хостов.

```json
"AllowedHosts": "*"
```

**Значение:**
- `*` - разрешены все хосты


