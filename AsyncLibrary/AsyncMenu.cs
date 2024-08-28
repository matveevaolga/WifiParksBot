using CHW3_3Library;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace AsyncLibrary
{
    public class AsyncMenu
    {
        ITelegramBotClient botClient;
        CancellationToken cancellationToken;

        // Callback запросы, отсылаемые ботом, которые необходимо обработать
        enum CallbackQueries
        {
            UploadJson,
            UploadCsv,
            DownloadJson,
            DownloadCsv,
            CoverageAreaFilter,
            ParkNameFilter,
            AdmAndCoverageAreaFilter,
            NoCallback
        }
        // Текущий обрабатываемый callback
        CallbackQueries currentCallback = CallbackQueries.NoCallback;
        JSONProcessing jsonProcessing;
        CSVProcessing csvProcessing;
        // WifiList-представление последнего отправленного на обработку пользователем файла
        WifiList? lastUploaded = null;
        // Сообщение, ожидающее ответа пользователя (для фильтрации)
        Message? messageToReplyTo = null;
        public AsyncHelpingFunctions HelpingFunctions { get; }
        ILogger logger;

        public AsyncMenu(ITelegramBotClient botClient, CancellationToken cancellationToken,
                         ILogger logger)
        {
            this.botClient = botClient;
            this.cancellationToken = cancellationToken;
            jsonProcessing = new JSONProcessing();
            csvProcessing = new CSVProcessing();
            HelpingFunctions = new AsyncHelpingFunctions(botClient, cancellationToken);
            this.logger = logger;
        }

        /// <summary>
        /// Обработка отправки пользователем текстового сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task TextMessageProcessing(Message message)
        {
            if (message.Type != MessageType.Text)
            {
                currentCallback = CallbackQueries.NoCallback;
                await HelpingFunctions.SendDefaultInfo(message.Chat);
                logger.LogError($"Получено сообщение некорректного типа от пользователя {message.Chat.Id} в {DateTime.Now}");
                return;
            }
            switch (message.Text)
            {
                case "/help":
                    currentCallback = CallbackQueries.NoCallback;
                    await HelpingFunctions.SendDefaultInfo(message.Chat);
                    logger.LogInformation($"Получена команда /help от пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case "/upload":
                    currentCallback = CallbackQueries.NoCallback;
                    await HelpingFunctions.SendUploadInline(message.Chat);
                    logger.LogInformation($"Получена команда /upload от пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case "/download":
                    currentCallback = CallbackQueries.NoCallback;
                    if (lastUploaded == null) { await NoLastUploaded(message); return; }
                    await HelpingFunctions.SendDownloadInline(message.Chat);
                    logger.LogInformation($"Получена команда /download от пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case "/sort":
                    currentCallback = CallbackQueries.NoCallback;
                    if (lastUploaded == null) { await NoLastUploaded(message); return; }
                    await HelpingFunctions.SendSortInline(message.Chat);
                    logger.LogInformation($"Получена команда /sort от пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case "/filter":
                    currentCallback = CallbackQueries.NoCallback;
                    if (lastUploaded == null) { await NoLastUploaded(message); return; }
                    await HelpingFunctions.SendFilterInline(message.Chat);
                    logger.LogInformation($"Получена команда /filter от пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                default:
                    logger.LogInformation($"Получено текстовое сообщение от пользователя {message.Chat.Id} в {DateTime.Now}");
                    await CheckIfReply(message);
                    break;
            }
            currentCallback = CallbackQueries.NoCallback;
        }

        /// <summary>
        /// Обработка отсутствия загруженного пользователем файла
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        async Task NoLastUploaded(Message message)
        {
            await HelpingFunctions.SendErrorMessage("Загрузите *json* или *csv* файл для обработки", message.Chat);
            logger.LogWarning($"Пользователь {message.Chat.Id} не загрузил файл перед запросом на скачивание или обработку в {DateTime.Now}");
        }

        /// <summary>
        /// Обработка отправки пользователем json-файла
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        async Task UploadJsonProcessing(Message message)
        {
            lastUploaded = null;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    if (!message.Document.FileName.EndsWith(".json"))
                        throw new ArgumentException();
                    var file = await botClient.GetInfoAndDownloadFileAsync(
                    fileId: message.Document.FileId,
                    destination: memoryStream,
                    cancellationToken: cancellationToken);
                    lastUploaded = jsonProcessing.Read(memoryStream);
                }
                catch (NullReferenceException) { lastUploaded = null; }
                catch (JsonException) { lastUploaded = null; }
                catch (ArgumentException) { lastUploaded = null; }
                catch (Exception) { lastUploaded = null; }
            }
            if (lastUploaded == null)
            {
                await HelpingFunctions.SendErrorMessage("Данные в файле некорректны", message.Chat);
                logger.LogError($"Не удалось считать данные из" +
                    $" json файла от пользователя {message.Chat.Id} в {DateTime.Now}");
                return;
            }
            await HelpingFunctions.SendSuccessMessage("Данные успешно считаны", message.Chat);
            logger.LogInformation($"Считаны данные из" +
                $" json файла от пользователя {message.Chat.Id} в {DateTime.Now}");
        }

        /// <summary>
        ///  Обработка отправки пользователем csv-файла
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        async Task UploadCsvProcessing(Message message)
        {
            lastUploaded = null;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    if (!message.Document.FileName.EndsWith(".csv"))
                        throw new ArgumentException();
                    var file = await botClient.GetInfoAndDownloadFileAsync(
                    fileId: message.Document.FileId,
                    destination: memoryStream,
                    cancellationToken: cancellationToken);
                    lastUploaded = csvProcessing.Read(memoryStream);
                }
                catch (NullReferenceException) { lastUploaded = null; }
                catch (JsonException) { lastUploaded = null; }
                catch (ArgumentException) { lastUploaded = null; }
                catch (Exception) { lastUploaded = null; }
            }
            if (lastUploaded == null)
            {
                await HelpingFunctions.SendErrorMessage("Данные в файле некорректны", message.Chat);
                logger.LogError($"Не удалось считать данные из" +
                                $" csv файла от пользователя {message.Chat.Id} в {DateTime.Now}");
                return;
            }
            await HelpingFunctions.SendSuccessMessage("Данные успешно считаны", message.Chat);
            logger.LogInformation($"Считаны данные из" +
                $" csv файла от пользователя {message.Chat.Id} в {DateTime.Now}");
        }

        /// <summary>
        /// Обработка отправки в чат документа пользователем
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public async Task DocumentMessageProcessing(Message message)
        {
            if (message.Type != MessageType.Document || message.Document == null)
            {
                await HelpingFunctions.SendDefaultInfo(message.Chat);
                currentCallback = CallbackQueries.NoCallback;
                logger.LogError($"Получено сообщение некорректного" +
                    $" типа от пользователя {message.Chat.Id} в {DateTime.Now}");
                return;
            }
            logger.LogInformation($"Получен документ от пользователя {message.Chat.Id} в {DateTime.Now}");
            switch (currentCallback)
            {
                case CallbackQueries.UploadJson:
                    await UploadJsonProcessing(message);
                    break;
                case CallbackQueries.UploadCsv:
                    await UploadCsvProcessing(message);
                    break;
                default:
                    await HelpingFunctions.SendDefaultInfo(message.Chat);
                    break;
            }
            currentCallback = CallbackQueries.NoCallback;
        }

        /// <summary>
        /// Обработка выбора пользователем скачивания данных в формате csv
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        async Task DownloadCsvProcessing(CallbackQuery callbackQuery)
        {
            if (lastUploaded == null) { await NoLastUploaded(callbackQuery.Message); return; }
            using (Stream stream = csvProcessing.Write(lastUploaded))
            {
                await botClient.SendDocumentAsync(
                chatId: callbackQuery.Message.Chat.Id,
                document: InputFile.FromStream(stream: stream, fileName: "result.csv"),
                caption: "✨Обработанный файл");
            }
            await HelpingFunctions.SendSuccessSticker(callbackQuery.Message.Chat);
            logger.LogInformation($"Загрузка обработанного csv файла для пользователя" +
                $" {callbackQuery.From.Id} прошла успешно");
        }

        /// <summary>
        /// Обработка выбора пользователем скачивания данных в формате json
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        async Task DownloadJsonProcessing(CallbackQuery callbackQuery)
        {
            if (lastUploaded == null) { await NoLastUploaded(callbackQuery.Message); return; }
            using (Stream stream = jsonProcessing.Write(lastUploaded))
            {
                await botClient.SendDocumentAsync(
                chatId: callbackQuery.Message.Chat.Id,
                document: InputFile.FromStream(stream: stream, fileName: "result.json"),
                caption: "✨Обработанный файл");
            }
            await HelpingFunctions.SendSuccessSticker(callbackQuery.Message.Chat);
            logger.LogInformation($"Загрузка обработанного json файла для пользователя" +
                $" {callbackQuery.From.Id} прошла успешно");
        }

        /// <summary>
        /// Обработка получения Callback запроса
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public async Task CallbackMessageProcessing(CallbackQuery callbackQuery)
        {
            logger.LogInformation($"Получен CallbackQuery {callbackQuery.Data} от пользователя {callbackQuery.From.Id} в {DateTime.Now}");
            switch (callbackQuery.Data)
            {
                case "UploadJson":
                    currentCallback = CallbackQueries.UploadJson;
                    await HelpingFunctions.SendMessage("Загрузите *json* файл для обработки",
                          callbackQuery.Message.Chat);
                    break;
                case "UploadCsv":
                    currentCallback = CallbackQueries.UploadCsv;
                    await HelpingFunctions.SendMessage("Загрузите *csv* файл для обработки",
                          callbackQuery.Message.Chat);
                    break;
                case "DownloadJson":
                    await DownloadJsonProcessing(callbackQuery);
                    break;
                case "DownloadCsv":
                    await DownloadCsvProcessing(callbackQuery);
                    break;
                case "NameSort":
                    lastUploaded.Sort(WifiObject.Headers.Name);
                    await HelpingFunctions.SendMessage("Данные успешно отсрорированы", callbackQuery.Message.Chat);
                    logger.LogInformation($"Произошла сортировка по полю Name файла пользователя {callbackQuery.From.Id} в {DateTime.Now}");
                    break;
                case "CoverageAreaSort":
                    lastUploaded.Sort(WifiObject.Headers.CoverageArea);
                    await HelpingFunctions.SendMessage("Данные успешно отсортированы", callbackQuery.Message.Chat);
                    logger.LogInformation($"Произошла сортировка по полю CoverageArea файла пользователя {callbackQuery.From.Id} в {DateTime.Now}");
                    break;
                case "CoverageAreaFilter":
                    currentCallback = CallbackQueries.CoverageAreaFilter;
                    messageToReplyTo = await HelpingFunctions.SendMessage("__*Ответьте*__ на данное сообщение: " +
                        "введите значение *CoverageArea*, выборку с которым хотите",
                        callbackQuery.Message.Chat);
                    break;
                case "ParkNameFilter":
                    currentCallback = CallbackQueries.ParkNameFilter;
                    messageToReplyTo = await HelpingFunctions.SendMessage("__*Ответьте*__ на данное сообщение: " +
                        "введите значение *ParkName*, выборку с которым хотите",
                        callbackQuery.Message.Chat);
                    break;
                case "AdmAndCoverageAreaFilter":
                    currentCallback = CallbackQueries.AdmAndCoverageAreaFilter;
                    messageToReplyTo = await HelpingFunctions.SendMessage("__*Ответьте*__ на данное сообщение: " +
                        "введите в две строки значения *AdmArea* и *CoverageArea*, выборку с которыми хотите " +
                        "\\(спуститься на новую строку можно с помощью *Shift\\+Enter*\\)",
                        callbackQuery.Message.Chat);
                    break;
                default:
                    currentCallback = CallbackQueries.NoCallback;
                    await HelpingFunctions.SendErrorMessage("Невозможно совершить данное действие",
                        callbackQuery.Message.Chat);
                    logger.LogWarning($"Не удалось обработать Callback типа {callbackQuery} от пользователя {callbackQuery.From.Id} в {DateTime.Now}");
                    break;
            }
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        /// <summary>
        /// Обработка выбора фильтрации по полям AdmArea и CoverageArea
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        async Task AdmAndCoverageAreaFilterProcessing(Message message)
        {
            if (message.Text.Split('\n').GetLength(0) != 2)
            {
                await HelpingFunctions.SendErrorMessage("Введены некорректные данные",
                message.Chat);
                logger.LogError($"Пользователь {message.From.Id} ввел некорректные" +
                    $" данные для фильтрации в {DateTime.Now}");
                return;
            }
            lastUploaded.Select(WifiObject.Headers.AdmArea,
                message.Text.Split('\n')[0]);
            lastUploaded.Select(WifiObject.Headers.CoverageArea,
                message.Text.Split('\n')[1]);
            await HelpingFunctions.SendMessage("Данные успешно отфильтрованы", message.Chat);
            logger.LogInformation($"Произошла фильтрация по полям " +
                $"AdmArea и CoverageArea файла пользователя {message.Chat.Id} в {DateTime.Now}");
        }

        /// <summary>
        /// Обработка выбора пользователем фильтрации
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="AggregateException"></exception>
        async Task FilterProcessing(Message message)
        {
            switch (currentCallback)
            {
                case (CallbackQueries.CoverageAreaFilter):
                    lastUploaded.Select(WifiObject.Headers.CoverageArea, message.Text);
                    await HelpingFunctions.SendMessage("Данные успешно отфильтрованы", message.Chat);
                    logger.LogInformation($"Произошла фильтрация по полю CoverageArea файла" +
                        $" пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case (CallbackQueries.ParkNameFilter):
                    lastUploaded.Select(WifiObject.Headers.ParkName, message.Text);
                    await HelpingFunctions.SendMessage("Данные успешно отфильтрованы", message.Chat);
                    logger.LogInformation($"Произошла фильтрация по полю ParkName" +
                        $" файла пользователя {message.Chat.Id} в {DateTime.Now}");
                    break;
                case (CallbackQueries.AdmAndCoverageAreaFilter):
                    await AdmAndCoverageAreaFilterProcessing(message);
                    break;
                default:
                    await HelpingFunctions.SendDefaultInfo(message.Chat);
                    currentCallback = CallbackQueries.NoCallback;
                    logger.LogError($"Пользователь {message.Chat.Id} ввел некорректные" +
                        $" данные для фильтрации в {DateTime.Now}");
                    return;
            }
        }

        /// <summary>
        /// Проверка, является ли отправленное пользователем
        /// текстовое сообщени ответом на messageToReplyTo
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AggregateException"></exception>
        async Task CheckIfReply(Message message)
        {
            try
            {
                if (message.ReplyToMessage.MessageId
                    != messageToReplyTo.MessageId)
                    throw new ArgumentNullException();
                await botClient.EditMessageTextAsync(message.Chat.Id, messageToReplyTo.MessageId,
                    FormEditedMessage(messageToReplyTo.Text),
                    parseMode: ParseMode.MarkdownV2);
                logger.LogInformation($"Пользователь {message.Chat.Id} ответил на сообщение {messageToReplyTo.MessageId}" +
                    $" данными для фильтации в {DateTime.Now}");
                await FilterProcessing(message);
            }
            catch (AggregateException) { await HelpingFunctions.SendDefaultInfo(message.Chat); }
            catch (IndexOutOfRangeException) { await HelpingFunctions.SendDefaultInfo(message.Chat); }
            catch (NullReferenceException) { await HelpingFunctions.SendDefaultInfo(message.Chat); }
            catch (ArgumentException) { await HelpingFunctions.SendDefaultInfo(message.Chat); }
            catch (Exception) { await HelpingFunctions.SendDefaultInfo(message.Chat); }
            currentCallback = CallbackQueries.NoCallback;
            messageToReplyTo = null;
        }

        /// <summary>
        /// Редактирование текста сообщения messageToReplyTo после ответа пользователем
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        string FormEditedMessage(string text)
        {
            return (text + " ✅").
                    Replace("Ответьте", "__*Ответьте*__").
                    Replace("ParkName", "*ParkName*").
                    Replace("AdmArea", "*AdmArea*").
                    Replace("CoverageArea", "*CoverageArea*").
                    Replace("Shift+Enter", "*Shift\\+Enter*\\").
                    Replace("(", "\\(");
        }

        /// <summary>
        /// Сброс меню в случае непредвиденной ошибки
        /// </summary>
        public void ResetMenu()
        {
            currentCallback = CallbackQueries.NoCallback;
            messageToReplyTo = null;
            lastUploaded = null;
        }
    }
}
