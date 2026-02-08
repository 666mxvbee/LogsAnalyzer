# Logs Analyzer

Консольная утилита для анализа **NGINX access-логов** и генерации отчётов в форматах **JSON / Markdown / AsciiDoc**.

## Возможности
- Источники логов:
    - локальный файл по пути
    - glob-паттерны (например, `logs/*.log`)
    - удалённые файлы по URL
    - несколько файлов одновременно
- Фильтрация по датам (`--from`, `--to`) в формате **ISO8601**
- Потоковая обработка (построчно, без загрузки всего файла в память)
- Логирование через **Serilog** в stdout (WARN для некорректных строк, INFO/ERROR/FATAL для важных событий)

## Собираемая статистика
- общее количество запросов
- размер ответа: **средний (точность 2 знака)**, **максимальный**, **95% перцентиль (p95)**
- частота кодов ответа
- топ-10 наиболее часто запрашиваемых ресурсов
- распределение запросов по датам в процентах
- уникальные протоколы

## Запуск

### 1. анализ удалённого NGINX лога → AsciiDoc отчёт
**Ввод:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path https://raw.githubusercontent.com/elastic/examples/master/Common%20Data%20Formats/nginx_logs/nginx_logs \
  --format adoc \
  --output report.adoc
```
**Вывод:** [report.adoc](examples/report.adoc)

### 2. анализ локальных файлов → JSON отчёт
**Ввод:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path ./scripts/data/input/logs/part1.txt ./scripts/data/input/logs/part1.txt \
  --format json \
  --output full_report.json
```
**Вывод:** [report.json](examples/full_report.json)

### 3. анализ локальных логов → Markdown отчёт
**Ввод:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path ./scripts/data/input/logs/part1.txt ./scripts/data/input/logs/part1.txt \
  --format markdown \
  --output full_report.md
```
**Вывод:** [report.md](examples/full_report.md)