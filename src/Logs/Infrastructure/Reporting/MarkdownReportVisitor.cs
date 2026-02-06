using System.Text;
using Logs.Core.Entities;
using Logs.Core.Interfaces;

namespace Logs.Infrastructure.Reporting;

public sealed class MarkdownReportVisitor : IReportVisitor
{
    private string _result = string.Empty;

    public void Visit(LogStatistics statistics)
    {
        var report = statistics.Calculate();
        var sb = new StringBuilder();

        sb.AppendLine("#### Общая информация");
        sb.AppendLine();
        sb.AppendLine("|        Метрика        |     Значение |");
        sb.AppendLine("|:---------------------:|-------------:|");
        sb.AppendLine($"|       Файл(-ы)        | `{string.Join(", ", report.FileNames)}` |");
        sb.AppendLine($"|  Количество запросов  |       {report.TotalRequests} |");
        sb.AppendLine($"| Средний размер ответа |         {report.AverageResponseSize:F0}b |");
        sb.AppendLine($"|  95p размера ответа   |         {report.P95ResponseSize}b |");
        sb.AppendLine();

        sb.AppendLine("#### Запрашиваемые ресурсы");
        sb.AppendLine("|     Ресурс      | Количество |");
        sb.AppendLine("|:---------------:|-----------:|");
        foreach (var item in report.TopResources)
        {
            sb.AppendLine($"|  `{item.Key}`  |      {item.Value} |");
        }
        sb.AppendLine();

        sb.AppendLine("#### Коды ответа");
        sb.AppendLine("| Код | Количество |");
        sb.AppendLine("|:---:|-----------:|");
        foreach (var item in report.ResponseCodes)
        {
            sb.AppendLine($"| {item.Key} |       {item.Value} |");
        }
        sb.AppendLine();

        if (report.RequestsPerDate.Any())
        {
            sb.AppendLine("#### Распределение запросов по датам");
            sb.AppendLine("| Дата | День недели | Количество | Процент |");
            sb.AppendLine("|:----:|:-----------:|-----------:|--------:|");
            foreach (var item in report.RequestsPerDate.OrderBy(x => x.Key))
            {
                var percentage = report.TotalRequests > 0
                    ? Math.Round((double)item.Value / report.TotalRequests * 100, 2)
                    : 0;
                sb.AppendLine($"| {item.Key:yyyy-MM-dd} | {item.Key.DayOfWeek} | {item.Value} | {percentage}% |");
            }
            sb.AppendLine();
        }

        if (report.UniqueProtocols.Any())
        {
            sb.AppendLine("#### Уникальные протоколы");
            sb.AppendLine(string.Join(", ", report.UniqueProtocols.OrderBy(x => x)));
        }

        _result = sb.ToString();
    }

    public string GetResult()
        => _result;

    public string GetExtension()
        => ".md";
}