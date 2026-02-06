namespace Logs.Core.ValueObjects;

public record DateRange(DateTimeOffset? From, DateTimeOffset? To)
{
    public bool IsInRange(DateTimeOffset date)
    {
        if (From.HasValue && date < From.Value)
        {
            return false;
        }

        if (To.HasValue && date > To.Value)
        {
            return false;
        }

        return true;
    }
}