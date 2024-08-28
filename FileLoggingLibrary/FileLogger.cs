using Microsoft.Extensions.Logging;

namespace FileLoggingLibrary
{
    /// <summary>
    /// Класс для логирования сообщений
    /// о работе программы в файл, реализует интерфейс ILogger
    /// </summary>
    public class FileLogger : ILogger<FileLogger>
    {
        const string _filePath = "../../../../var/log.txt";
        string _categoryName;

        /// <summary>
        /// Конструктор, принимающий путь к файлу для
        /// логирования и имя категории логирования
        /// </summary>
        /// <param name="path"></param>
        /// <param name="categoryName"></param>
        public FileLogger(string categoryName) => this._categoryName = categoryName;

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Запись лога в файл
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Log<TState>(LogLevel logLevel, EventId eventId,
                    TState state, Exception exception, Func<TState,
                    Exception, string> formatter)
        {
            if (formatter != null)
                File.AppendAllText(_filePath,
                $"{logLevel}: {_categoryName}" + Environment.NewLine + "\t" +
                formatter(state, exception) + Environment.NewLine);
        }
    }
}
