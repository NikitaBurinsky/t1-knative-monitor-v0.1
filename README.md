# T1 Knative Administrator

Система мониторинга и управления Knative-функциями с биллингом и метриками в реальном времени.

## Функциональность

- **Мониторинг метрик**: Сбор и отображение CPU, памяти, количества запросов и времени выполнения
- **Биллинг в реальном времени**: Расчет стоимости выполнения функций на основе использования ресурсов
- **Управление функциями**: Запуск функций с настраиваемыми параметрами
- **Knative интеграция**: Автоматический сбор метрик из Knative Serving
- **Prometheus мониторинг**: Интеграция с Prometheus для сбора метрик
- **Веб-интерфейс**: Дашборд с возможностью теста фич, а также поддержка OpenApi | SwaggerUI (/swagger/index.html)

## Технологический стек

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, Kubernetes Client
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **База данных**: In-Memory Database (Экранированна EF Core, позволяет легкую интеграцию любых СУБД)
- **Мониторинг**: Prometheus
- **Оркестрация**: Kubernetes, Knative Serving
- **Контейнеризация**: Docker

## Быстрый старт

### Предварительные требования

- Kubernetes кластер с Knative и kube-prometheus-stack
- Docker

### Установка

1. **Клонирование репозитория**

```
git clone https://github.com/NikitaBurinsky/t1-knative-monitor-v0.1.git
```

2. Сборка и запуск

Требуется собрать Docker-образ и запустить его, открыв для него локальный порт,
на котором запущен Kubernetes кластер. Самые простой способ это сделать - использовать
`--host` флаг:

```
docker build -t <name> .
docker run --host <name>:latest
```

3. Настрйка Prometheus

```
kubectl apply -f service-monitor.yaml
```

4. Доступ к приложению

- Веб-интерфейс: [http://localhost:8080/index.html](http://localhost:8080/index.html)
- Swagger UI: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

## Конфигурация и настройка тарифных планов

Описание конфигураций приложения находится в [CONFIGURATION.md](CONFIGURATION.md). 
Все настройки находятся в файле [appsettings.json](./appsettings.json) в корне репозитория.

## Архитекутра

Архитектура описана в [ARCHITECTURE.md](ARCHITECTURE.md).
