using Microsoft.Extensions.Logging;

namespace FileLoggingLibrary
{
    /// <summary>
    /// Статический класс с функциями,
    /// помогающими совершить логирование в файл
    /// </summary>
    public static class FileLoggerHelpingFunctions
    {
        /// <summary>
        /// Добавление провайдера, совершающего запись в файл, к переданной фабрике логов
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static ILoggerFactory StartLoggingToFile(ILoggerFactory factory)
        {
            factory.AddProvider(new FileLoggerProvider());
            return factory;
        }
    }
}
