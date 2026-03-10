# Logs Analyzer

A console utility for analyzing **NGINX access logs** and generating reports in **JSON / Markdown / AsciiDoc** formats.

## Features

### Log sources
- local file by path  
- glob patterns (e.g. `logs/*.log`)  
- remote files via URL  
- multiple files at once  

### Other capabilities
- Date filtering (`--from`, `--to`) in **ISO8601** format  
- Streaming processing (line-by-line without loading the whole file into memory)  
- Logging via **Serilog** to stdout:
  - `WARN` for malformed log lines
  - `INFO / ERROR / FATAL` for important events

## Collected Statistics
- total number of requests  
- response size: **average (precision: 2 decimal places)**, **maximum**, **95th percentile (p95)**  
- response status code frequency  
- top 10 most frequently requested resources  
- request distribution by date (in percentages)  
- unique protocols
  
## Running

### 1. Analyze remote NGINX log → AsciiDoc report
**Input:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path https://raw.githubusercontent.com/elastic/examples/master/Common%20Data%20Formats/nginx_logs/nginx_logs \
  --format adoc \
  --output report.adoc
```
**Output:** [report.adoc](examples/report.adoc)

### 2. Analyze local files → JSON report
**Input:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path ./scripts/data/input/logs/part1.txt ./scripts/data/input/logs/part1.txt \
  --format json \
  --output full_report.json
```
**Output:** [report.json](examples/full_report.json)

### 3. Analyze local logs → Markdown report
**Input:**
```bash
dotnet run --project ./src/Logs/Logs.csproj -- \
  --path ./scripts/data/input/logs/part1.txt ./scripts/data/input/logs/part1.txt \
  --format markdown \
  --output full_report.md
```
**Output:** [report.md](examples/full_report.md)
