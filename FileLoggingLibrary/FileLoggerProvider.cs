using Microsoft.Extensions.Logging;

namespace FileLoggingLibrary
{
    /// <summary>
    /// Класс, представляющий провайдер логгирования для записи в файл.
    /// Он реализовывает интерфейс ILoggerProvider
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
            => new FileLogger(categoryName);

        /// <summary>
        /// Управляет освобождение ресурсов
        /// </summary>
        public void Dispose() { }
    }
}
