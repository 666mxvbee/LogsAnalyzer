using Logs.Infrastructure.Cli;
using Serilog;

namespace Logs;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var parser = new ArgParser();
            AnalyzeCommand command = parser.Parse(args);

            command.Execute();
            return 0;
        }
        catch (ArgumentException e)
        {
            Log.Error("Invalid arguments: {Message}", e.Message);
            return 2;
        }
        catch (FileNotFoundException e)
        {
            Log.Error("File not found: {Message}", e.Message);
            return 2;
        }
        catch (IOException e)
        {
            Log.Error("IO error: {Message}", e.Message);
            return 2;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Unexpected error occurred");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}