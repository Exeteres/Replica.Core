using System;
using Microsoft.Extensions.Logging;
using VkNet;

namespace Replica.Controllers.VK.Utils
{
    public class SerilogLogger : ILogger<VkApi>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = "[VK] " + formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Information:
                    Serilog.Log.Information(message);
                    break;
                case LogLevel.Trace:
                    Serilog.Log.Verbose(message);
                    break;
                case LogLevel.Debug:
                    Serilog.Log.Debug(message);
                    break;
                case LogLevel.Error:
                    Serilog.Log.Error(exception, message);
                    break;
                case LogLevel.Warning:
                    Serilog.Log.Warning(message);
                    break;
                case LogLevel.Critical:
                    Serilog.Log.Fatal(exception, message);
                    break;
            }
        }
    }
}