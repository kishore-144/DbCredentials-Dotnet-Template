using Microsoft.Extensions.Logging;
namespace DbCredentials.LoggingClass
{

    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();
        private readonly string _categoryName;

        public FileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        private readonly string _projectName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
    TState state, Exception? exception,
    Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var now = DateTimeOffset.Now;
            string message;

            try
            {
                message = formatter(state, exception);
            }
            catch
            {
                message = state?.ToString() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(message))
                return;

            var logLine = $"{logLevel}: {_categoryName}[{eventId.Id}]"
                        + $" {now:yyyy-MM-dd HH:mm:ss zzz}"
                        + Environment.NewLine
                        + $"      {message}"
                        + Environment.NewLine
                        + Environment.NewLine;

            if (exception != null)
                logLine += Environment.NewLine + exception;

            lock (_lock)
            {
                File.AppendAllText(_filePath, logLine + Environment.NewLine);
            }
        }


    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName) =>
            new FileLogger(_filePath, categoryName);

        public void Dispose() { }
    }
}
