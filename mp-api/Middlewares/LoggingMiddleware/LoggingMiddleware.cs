

namespace mp_api
{
    #region using

    using logger.util;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    #endregion


    public static class LoggingMiddleware
    {

        public static ILoggingBuilder InitLoggerConfiguration(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddFile(configuration);
            return loggingBuilder;
        }
    }
}
