

namespace logger.util
{

    #region using

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.IO;

    #endregion


    public static class LoggerExtension
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder loggingBuilder, IConfiguration configurationSection)
        {
            if (loggingBuilder == null)
            {
                throw new ArgumentNullException(nameof(loggingBuilder));
            }

            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            var minimumLevel = LogLevel.Information;
            var levelSection = configurationSection["Logging:LogLevel"];
            if (!string.IsNullOrEmpty(levelSection))
            {
                if (!Enum.TryParse(levelSection, out minimumLevel))
                {
                    Debug.WriteLine("The minimum level {0} is invalid.", levelSection);
                }
            }
            var filePath = string.IsNullOrEmpty(configurationSection["Logging:FilePath"]) 
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"mp.log")
                : configurationSection["Logging:FilePath"];
            return loggingBuilder.AddFile(filePath, (categoryName, logLevel) => { return logLevel >= minimumLevel; });
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder loggingBuilder, string filePath, LogLevel minimumLevel = LogLevel.Information)
        {
            return loggingBuilder.AddFile(filePath, (categoryName, logLevel) => { return logLevel >= minimumLevel; });
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder loggingBuilder, string filePath, Func<string, LogLevel, bool> filter)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            loggingBuilder.AddProvider(new LoggerProvider(filter, filePath));
            return loggingBuilder;
        }
    }
}
