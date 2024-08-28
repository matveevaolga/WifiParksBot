using AsyncLibrary;
using FileLoggingLibrary;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CHW3_3
{
    public class BotRunner
    {
        const string token = "6871148388:AAHonMHc7jTIs3UZKOof5em5gklHCQ6QQCE";
        static CancellationTokenSource cancellTokenSource = new();
        static ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = new[] { UpdateType.Message,
                                     UpdateType.CallbackQuery }
        };

        ILogger logger;
        ITelegramBotClient botClient;
        // фабрика логгеров ))
        ILoggerFactory loggerFactory;
        // Словарь с AsyncMenu пользователей бота
        Dictionary<long, AsyncMenu> usersAsyncMenus = new Dictionary<long, AsyncMenu>();

        /// <summary>
        /// Создание объекта бота
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public BotRunner() => botClient = new TelegramBotClient(token);

        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            try
            {
                using (ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole()))
                {
                    loggerFactory = factory;
                    FileLoggerHelpingFunctions.StartLoggingToFile(factory);
                    logger = factory.CreateLogger("BotRunner");
                    botClient.StartReceiving(
                            updateHandler: HandleUpdateAsync,
                            pollingErrorHandler: HandlePollingErrorAsync,
                            receiverOptions: receiverOptions,
                            cancellationToken: cancellTokenSource.Token
                        );
                    var me = await botClient.GetMeAsync();
                    logger.LogInformation($"Start listening for @{me.Username} at {DateTime.Now}");
                    Console.ReadLine();
                    cancellTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"ObjectDisposedException ошибка в Main");
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"NullReferenceException ошибка в Main");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная ошибка" +
                $"{ex.GetType().Name} в Main" +
                " (возможно, связана с логированием => запись в лог опасна...)");
            }
        }

        /// <summary>
        /// Запись сообщение об ошибке, возникшей в HandleUpdateAsync в лог
        /// </summary>
        /// <param name="exceptionName"></param>
        /// <param name="messageType"></param>
        /// <param name="userId"></param>
        void HandlerLogError(string exceptionName, string messageType, long userId) =>
                logger.LogCritical($"Произошла ошибка {exceptionName} во время" +
                    $" обработки ботом {messageType} в {DateTime.Now} от пользователя {userId}");

        /// <summary>
        /// Получение AsyncMenu пользователя бота
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        AsyncMenu GetUserAsyncMenu(long userId)
        {
            if (!usersAsyncMenus.ContainsKey(userId))
            {
                ILogger userLogger = loggerFactory.CreateLogger($"AsyncMenuFor{userId}");
                AsyncMenu userMenu = new AsyncMenu(botClient, cancellTokenSource.Token, userLogger);
                usersAsyncMenus.Add(userId, userMenu);
            }
            return usersAsyncMenus[userId];
        }

        /// <summary>
        /// Обработка update-ов
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient,
            Update update, CancellationToken cancellationToken)
        {
            try
            {
                AsyncMenu asyncMenu;
                switch (update.Type)
                {
                    case UpdateType.Message:
                        Message? message = update.Message;
                        if (message == null)
                        { logger.LogCritical($"Получено null Message в Update {update.Id}"); return; }
                        asyncMenu = GetUserAsyncMenu(message.Chat.Id);
                        switch (message.Type)
                        {
                            case MessageType.Text:
                                try { await asyncMenu.TextMessageProcessing(message); }
                                catch (Exception ex)
                                {
                                    HandlerLogError(ex.GetType().Name, "текстового сообщения", message.Chat.Id);
                                    asyncMenu.ResetMenu();
                                }
                                break;
                            case MessageType.Document:
                                try { await asyncMenu.DocumentMessageProcessing(message); }
                                catch (Exception ex)
                                {
                                    HandlerLogError(ex.GetType().Name, "сообщения с документом", message.Chat.Id);
                                    asyncMenu.ResetMenu();
                                }
                                break;
                            default:
                                logger.LogWarning($"Получено сообщение необрабатываемого типа от {message.Chat.Id}" +
                                    $" в {DateTime.Now}");
                                break;
                        }
                        break;
                    case UpdateType.CallbackQuery:
                        CallbackQuery? callback = update.CallbackQuery;
                        asyncMenu = GetUserAsyncMenu(callback.Message.Chat.Id);
                        if ((callback) == null)
                        { logger.LogCritical($"Получено null CallbackQuery в Update {update.Id}"); return; }
                        try { await asyncMenu.CallbackMessageProcessing(callback); }
                        catch (Exception ex)
                        {
                            HandlerLogError(ex.GetType().Name, "сообщения",
                                               callback.From.Id); asyncMenu.ResetMenu();
                        }
                        break;
                    default:
                        logger.LogWarning($"Получен update необрабатываемого типа в {DateTime.Now}"); 
                        break;
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"ArgumentException ошибка в HandleUpdateAsync");
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"NullReferenceException ошибка в HandleUpdateAsync");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная ошибка" +
                $"{ex.GetType().Name} в HandleUpdateAsync" +
                " (возможно, связана с логированием => запись в лог опасна...)");
            }
        }

        /// <summary>
        /// Обработка ошибок поллинга
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient,
            Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogCritical($"Критическая ошибка в HandlePollingErrorAsync в {DateTime.Now}");
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"NullReferenceException ошибка в HandlePollingErrorAsync");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"В {DateTime.Now} произошла непредвиденная" +
                $"{ex.GetType().Name} ошибка в HandlePollingErrorAsync" +
                " (возможно, связана с логированием => запись в лог опасна...)");
            }
            return Task.CompletedTask;
        }
    }
}
