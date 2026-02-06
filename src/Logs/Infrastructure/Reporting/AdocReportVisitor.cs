using System.Text;
using Logs.Core.Entities;
using Logs.Core.Interfaces;

namespace Logs.Infrastructure.Reporting;

public sealed class AdocReportVisitor : IReportVisitor
{
    private string _result = string.Empty;

    public void Visit(LogStatistics statistics)
    {
        var report = statistics.Calculate();
        var sb = new StringBuilder();

        sb.AppendLine("== Отчет анализа логов");
        sb.AppendLine();
        sb.AppendLine(".Общая информация");
        sb.AppendLine("|===");
        sb.AppendLine("|Метрика |Значение");
        sb.AppendLine();
        sb.AppendLine($"|Количество запросов |{report.TotalRequests}");
        sb.AppendLine($"|Средний размер ответа |{report.AverageResponseSize:F0}b");
        sb.AppendLine($"|95р размера ответа |{report.P95ResponseSize}b");
        sb.AppendLine("|===");

        sb.AppendLine();
        sb.AppendLine(".Ресурсы (Топ-10)");
        sb.AppendLine("|===");
        sb.AppendLine("|Ресурс |Количество");
        foreach (var item in report.TopResources)
        {
            sb.AppendLine($"|{item.Key} |{item.Value}");
        }
        sb.AppendLine("|===");
        sb.AppendLine();

        if (report.RequestsPerDate.Any())
        {
            sb.AppendLine(".Распределение запросов по датам");
            sb.AppendLine("|===");
            sb.AppendLine("|Дата |День недели |Количество |Процент");
            foreach (var item in report.RequestsPerDate.OrderBy(x => x.Key))
            {
                var percentage = report.TotalRequests > 0
                    ? Math.Round((double)item.Value / report.TotalRequests * 100, 2)
                    : 0;
                sb.AppendLine($"|{item.Key:yyyy-MM-dd} |{item.Key.DayOfWeek} |{item.Value} |{percentage}%");
            }
            sb.AppendLine("|===");
            sb.AppendLine();
        }

        if (report.UniqueProtocols.Any())
        {
            sb.AppendLine(".Уникальные протоколы");
            sb.AppendLine(string.Join(", ", report.UniqueProtocols.OrderBy(x => x)));
        }

        _result = sb.ToString();
    }

    public string GetResult()
        => _result;

    public string GetExtension()
        => ".adoc";
}