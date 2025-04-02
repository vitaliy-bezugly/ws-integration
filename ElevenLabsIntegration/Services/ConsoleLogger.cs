namespace ElevenLabsIntegration.Console.Services;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        System.Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}
