

namespace logger.util
{
    #region using

    using Microsoft.Extensions.Logging;
    using System;

    #endregion


    public class LoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> filter;
        private string fileName;
        public LoggerProvider(Func<string, LogLevel, bool> filter, string fileName)
        {
            this.filter = filter;
            this.fileName = fileName;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerHelper(categoryName, this.filter, this.fileName, this);
        }

        public void Dispose()
        {
            return;
        }
    }
}
