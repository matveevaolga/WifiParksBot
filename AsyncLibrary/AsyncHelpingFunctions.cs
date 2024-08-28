using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AsyncLibrary
{
    /// <summary>
    /// Класс с функциями, помогающими боту взаимодействовать с пользователем
    /// </summary>
    public class AsyncHelpingFunctions
    {
        ITelegramBotClient botClient;
        CancellationToken cancellationToken;

        public AsyncHelpingFunctions(ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            this.botClient = botClient;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Отправка Default стикера в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendDefaultSticker(Chat chat)
        {
            string stickerId = "CAACAgIAAxkBAAEqU8Jl-HhpoWXxfiwy439knRHEdHgG7QACDhoAApyb-Ustq5zGUsoOaTQE";
            await botClient.SendStickerAsync(
            chatId: chat.Id,
            disableNotification: true,
            sticker: InputFile.FromFileId(stickerId),
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка Success стикера в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendSuccessSticker(Chat chat)
        {
            string stickerId = "CAACAgIAAxkBAAEqU8xl-Hw_YNQEca-F6IplBmV_C3PJjQACwhQAArP70Eg4qTcl7eNzqjQE";
            await botClient.SendStickerAsync(
            chatId: chat.Id,
            disableNotification: true,
            sticker: InputFile.FromFileId(stickerId),
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка Error стикера в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendErrorSticker(Chat chat)
        {
            string stickerId = "CAACAgIAAxkBAAEqVRxl-KC-oFK7Rf00qdTw_DGN4DChoAACchcAAoL78Et6TFFCJ0bHwjQE";
            await botClient.SendStickerAsync(
            chatId: chat.Id,
            disableNotification: true,
            sticker: InputFile.FromFileId(stickerId),
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка текстового сообщения в чат
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task<Message> SendMessage(string text, Chat chat)
        {
            return await botClient.SendTextMessageAsync(
            chatId: chat.Id,
            text: text,
            parseMode: ParseMode.MarkdownV2,
            disableNotification: true,
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка в чат информации о функционале бота
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task SendDefaultInfo(Chat chat)
        {
            var description = botClient.GetMyDescriptionAsync().Result.Description;
            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: description.ToString().Replace(".", "\\.").Replace("-", "\\-"),
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                cancellationToken: cancellationToken);

            await SendDefaultSticker(chat);
        }

        /// <summary>
        /// Отправка в чат информации о возникшей ошибке
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task SendErrorMessage(string text, Chat chat)
        {
            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: text.Replace(".", "\\.").Replace("-", "\\-"),
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                cancellationToken: cancellationToken);

            await SendErrorSticker(chat);
        }

        /// <summary>
        /// Отправка в чат информации об успешном завершении операции
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task SendSuccessMessage(string text, Chat chat)
        {
            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: text.Replace(".", "\\.").Replace("-", "\\-"),
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                cancellationToken: cancellationToken);

            await SendSuccessSticker(chat);
        }

        /// <summary>
        /// Отправка клавиатуры с выбором поля для сортировки в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendSortInline(Chat chat)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Name", callbackData: "NameSort"),
                    InlineKeyboardButton.WithCallbackData(text: "CoverageArea", callbackData: "CoverageAreaSort"),
                },
            });
            await botClient.SendTextMessageAsync(
            chatId: chat.Id,
            disableNotification: true,
            text: "Выберите поле, по которому хотите отсортировать данные файла",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка клавиатуры с выбором поля для фильтрации в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendFilterInline(Chat chat)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "CoverageArea", callbackData: "CoverageAreaFilter"),
                    InlineKeyboardButton.WithCallbackData(text: "ParkName", callbackData: "ParkNameFilter"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "AdmArea и CoverageArea", callbackData: "AdmAndCoverageAreaFilter"),
                }
            });
            await botClient.SendTextMessageAsync(
            chatId: chat.Id,
            disableNotification: true,
            text: "Выберите поле, по которому хотите отфильтровать данные файла",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка клавиатуры с выбором расширения файла для скачивания в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendDownloadInline(Chat chat)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "json", callbackData: "DownloadJson"),
                    InlineKeyboardButton.WithCallbackData(text: "csv", callbackData: "DownloadCsv"),
                },
            });
            await botClient.SendTextMessageAsync(
            chatId: chat.Id,
            disableNotification: true,
            text: "Выберите, файл с каким расширением вы хотите скачать",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Отправка клавиатуры с выбором расширения файла для обработки в чат
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task SendUploadInline(Chat chat)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "json", callbackData: "UploadJson"),
                    InlineKeyboardButton.WithCallbackData(text: "csv", callbackData: "UploadCsv"),
                },
            });
            await botClient.SendTextMessageAsync(
            chatId: chat.Id,
            disableNotification: true,
            text: "Выберите, файл с каким расширением вы хотите обработать",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
        }
    }
}
