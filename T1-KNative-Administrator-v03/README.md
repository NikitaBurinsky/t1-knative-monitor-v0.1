# T1 Knative Administrator v03

Система мониторинга и управления Knative-функциями с биллингом и метриками в реальном времени.

## Функциональность

- **Мониторинг метрик**: Сбор и отображение CPU, памяти, количества запросов и времени выполнения
- **Биллинг в реальном времени**: Расчет стоимости выполнения функций на основе использования ресурсов
- **Управление функциями**: Запуск функций с настраиваемыми параметрами
- **Knative интеграция**: Автоматический сбор метрик из Knative Serving
- **Prometheus мониторинг**: Интеграция с Prometheus для сбора метрик
- **Веб-интерфейс**: Интуитивный дашборд с темной/светлой темой

## Технологический стек

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, Kubernetes Client
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **База данных**: In-Memory Database (с возможностью миграции)
- **Мониторинг**: Prometheus, Knative Metrics
- **Оркестрация**: Kubernetes, Knative Serving
- **Контейнеризация**: Docker

## Быстрый старт

### Предварительные требования

- .NET 8.0 SDK
- Kubernetes кластер с Knative
- Prometheus
- Docker

### Установка

1. **Клонирование репозитория**
   git clone <repository-url>
   cd T1-KNative-Administrator-v03

2. Сборка и запуск

    dotnet restore
    dotnet build
    dotnet run

3. Деплой в Kubernetes
    kubectl apply -f knative-service.yaml
    kubectl apply -f ServiceMonitor.yaml

4. Доступ к приложению
    Веб-интерфейс: http://localhost:5151

    API: http://localhost:5151/runner/echo

    Метрики: http://localhost:5151/metrics

    Swagger UI: http://localhost:5151/swagger

5. Использование
    - Мониторинг метрик
        Откройте веб-интерфейс для просмотра метрик в реального времени

        Метрики обновляются каждые 30 секунд автоматически

        Просматривайте CPU, память, количество запросов и время выполнения

    - Запуск функций
        Используйте форму "Запуск функции" для выполнения функций

        Настройте количество запусков (runTimes) и задержку (runDelay)

        Отслеживайте результаты выполнения в реальном времени

    - Биллинг
        Система автоматически рассчитывает стоимость выполнения функций

        Расчет основан на использовании CPU, памяти и количестве запросов

        Формулы расчета отображаются прозрачно в интерфейсе

⚙️ Конфигурация
Основные настройки (appsettings.json)
json
{
  "BillingConfigurations": {
    "Base": {
      "PricePerRequest": 0.0000002,
      "PricePerGBSeconds": 0.00001667,
      "PricePerVCpuSecond": 0.0405,
      "PricePerGBMemorySecond": 0.000001231
    }
  },
  "Seeding": {
    "FunctionsInfo": {
      "ServingName": "echo-00001",
      "RevisionName": "deployment", 
      "PODName": "5f657c6b6b-dkmhk"
    }
  }
}
📊 API Endpoints
GET /runner/echo/get-metrics
Получение текущих метрик функции для тестовой функции "echo-00001"

Response:

json
{
  "id": 1,
  "fullName": "echo-00001-deployment-5f657c6b6b-dkmhk",
  "revisionName": "deployment",
  "servingName": "echo-00001",
  "podName": "5f657c6b6b-dkmhk",
  "runTimeStats": {
    "maxRunningTimeMS": 150,
    "avgRunningTimeMS": 120,
    "metricsCounts": 10
  },
  "vCpuStats": { ... },
  "requestsCounterStats": { ... },
  "ramStats": { ... }
}
POST /runner/echo
Запуск функции с параметрами:

json
{
  "runTimes": 1,
  "runDelay": 0
}
🔧 Разработка
Структура проекта
text
T1-KNative-Administrator-v03/
├── Controllers/
│   └── FunctionRunnerController.cs
├── Core/
│   ├── Function/
│   │   └── FunctionEntity.cs
│   ├── OpResult/
│   │   └── OpResult.cs
│   └── Prometheus/
│       └── Response/
│           └── PrometheusQueryResponse.cs
├── Infrastructure/
│   ├── Repositories/
│   │   └── FunctionsInfoRepository/
│   │       └── FunctionsInfoRepository.cs
│   ├── Services/
│   │   └── FunctionsManagerService/
│   │       ├── FunctionsStatsManagerService.cs
│   │       └── WriteDataProfiles/
│   │           ├── BaseProfile/
│   │           │   └── BaseStatsWriterProfile.cs
│   │           └── RAMStatsProfile/
│   │               └── RAMStatsProfile.cs
│   └── Collectors/
│       └── KnativeControlMetricsCollector.cs
├── Program.cs
├── appsettings.json
└── wwwroot/
    ├── index.html
    ├── style.css
    └── script.js
Модели данных
FunctionEntity
Основная сущность функции содержит:

Идентификационные данные (Serving, Revision, Pod)

Метрики выполнения (CPU, RAM, Requests, Running Time)

Вложенные классы для различных типов метрик

OpResult
Унифицированная система обработки результатов операций с поддержкой:

Успешных и ошибочных сценариев

HTTP статус кодов

Сообщений об ошибках

Проблемных деталей (ProblemDetails)

Добавление новых метрик
Создайте новый профиль в WriteDataProfiles/, наследуясь от BaseStatsWriterProfile

Реализуйте метод WriteStatsMetric для обработки специфичных метрик

Добавьте запрос в KnativeControlMetricsCollector.servicesMetricQueries

Интегрируйте обработку в FunctionsStatsManagerService.WriteMetrics

🐛 Устранение неисправностей
Распространенные проблемы
Метрики не загружаются

Проверьте подключение к Prometheus на порту 9090

Убедитесь, что Knative службы работают

Проверьте корректность запросов в servicesMetricQueries

Ошибки Kubernetes

Проверьте kubeconfig configuration

Убедитесь в наличии доступа к кластеру

Проверьте RBAC настройки

Биллинг не рассчитывается

Проверьте конфигурацию цен в appsettings.json

Убедитесь в сборе всех необходимых метрик

Проверьте формулы расчета в script.js

Ошибки десериализации Prometheus

Убедитесь в корректности формата ответа Prometheus

Проверьте структуру PrometheusQueryResponse

🔮 Планы развития
Реализация дополнительных Writer Profiles для CPU и Request метрик

Добавление аутентификации и авторизации

Поддержка множественных функций

Расширенная визуализация исторических данных

Уведомления и алертинг

📄 Лицензия
Разработано командой KI14
Powered by Lucik U. A. spirit

*Примечание: В текущей реализации используется тестовая функция "echo-00001". Для работы с другими функциями требуется обновление конфигурации и кода.*

text