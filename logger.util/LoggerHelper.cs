

namespace logger.util
{
    #region using

    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion


    public class LoggerHelper : ILogger
    {
        private const long FILESIZE_MAX = 5;
        private const int FILECOUNT_MAX = 20;

        private string categoryName;
        private Func<string, LogLevel, bool> filter;
        private string fileName;
        private ILoggerProvider provider;
        private ReaderWriterLock locker_write = new ReaderWriterLock();
        private static readonly object locker_create = new object();

        public LoggerHelper(string categoryName, Func<string, LogLevel, bool> filter, string fileName, ILoggerProvider loggerProvider)
        {
            this.categoryName = categoryName;
            this.filter = filter;
            this.fileName = fileName;
            this.provider = loggerProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope(this.provider);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (this.filter == null || this.filter(this.categoryName, logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if(exception != null)
            {
                message += "\n" + exception.ToString();
            }

            this.InsertLog(message, eventId.Id, logLevel.ToString());
        }

        private async void InsertLog(string message, int eventId, string logLevel)
        {
            try
            {
                locker_write.AcquireWriterLock(int.MaxValue);
                await this.CheckDocumentRestriction();
                File.AppendAllText(fileName, $"{DateTime.Now} {eventId} {logLevel} {message} {Environment.NewLine}");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                locker_write.ReleaseWriterLock();
            }
        }

        private Task CheckDocumentRestriction()
        {
            try
            {
                var fileInfo = new FileInfo(this.fileName);
                if (!fileInfo.Exists)
                {
                    return Task.CompletedTask;
                }
                if (fileInfo.Length >= FILESIZE_MAX)
                {
                    var files = fileInfo.Directory.GetFiles().ToList();
                    files.Sort(new FileCompare());
                    for (var i = files.Count - 1; i > -1; i--)
                    {
                        if (i < FILECOUNT_MAX - 1)
                        {
                            var basePath = files[i].FullName.Substring(0, files[i].FullName.LastIndexOf("\\") + 1);
                            var oldFileName = files[i].FullName.Substring(basePath.Length, files[i].FullName.Length - basePath.Length);
                            var offset = oldFileName.LastIndexOf('.');
                            var defaultPartName = oldFileName.Substring(0, offset);
                            var firstPartName = oldFileName.LastIndexOf('_') > -1 ? oldFileName.Substring(0, oldFileName.LastIndexOf('_')) : defaultPartName;
                            var secondPartName = oldFileName.Substring(offset, oldFileName.Length - offset);
                            var newFullName = $"{basePath}{firstPartName}_{i + 1}{secondPartName}";
                            files[i].MoveTo(newFullName);
                        }
                        else
                        {
                            files[i].Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Task.CompletedTask;
        }

        private class Scope: IDisposable
        {
            private readonly IDisposable scope;

            public Scope(IDisposable scope)
            {
                this.scope = scope;
            }

            public void Dispose()
            {
                scope.Dispose();
            }
        }

        private class FileCompare : IComparer<FileInfo>
        {
            public int Compare(FileInfo left, FileInfo right)
            {
                if(left == null && right == null)
                {
                    return 0;
                }

                if(left == null)
                {
                    return -1;
                }

                if(right == null)
                {
                    return 1;
                }

                var leftArray = left.Name.Substring(0, left.Name.IndexOf('.')).Split('_');
                var rightArray = right.Name.Substring(0, right.Name.IndexOf('.')).Split('_');
                if(leftArray.Length == 1 && rightArray.Length == 1)
                {
                    var timePriority = DateTime.Compare(left.CreationTime, right.CreationTime);

                    if (timePriority > 0)
                    {
                        return 1;
                    }
                    else if (timePriority < 0)
                    {
                        return -1;
                    }
                }
                else if(leftArray.Length == 1)
                {
                    return 1;
                }else if(rightArray.Length == 1)
                {
                    return 1;
                }
                else
                {
                    return  Convert.ToInt32(leftArray[1]) - Convert.ToInt32(rightArray[1]);
                }

                return 0;
            }
        }
    }
}
