using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CHW3_3Library
{
    /// <summary>
    /// Абстрактный класс-представление записи в wifi-parks файле
    /// </summary>
    public abstract class WifiObject
    {
        public static readonly JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        /// <summary>
        /// Заголовки wifi-parks файла
        /// </summary>
        public enum Headers
        {
            ID = 0,
            GlobalId,
            Name,
            AdmArea,
            District,
            ParkName,
            WiFiName,
            CoverageArea,
            FunctionFlag,
            AccessFlag,
            Password,
            LongitudeWGS84,
            LatitudeWGS84,
            GeodataCenter,
            Geoarea
        }
        /// <summary>
        /// Словарь с русскими заголовками wifi-parks файла
        /// </summary>
        public static readonly IReadOnlyDictionary<Headers, string> russianHeaders =
            new Dictionary<Headers, string>()
        {
            { Headers.ID,  "Код" },
            { Headers.GlobalId, "global_id" },
            { Headers.Name, "Наименование" },
            { Headers.AdmArea, "Административный округ по адресу"},
            { Headers.District, "Район" },
            { Headers.ParkName, "Наименование парка" },
            { Headers.WiFiName , "Имя Wi-Fi сети" },
            { Headers.CoverageArea, "Зона покрытия (метры)" },
            { Headers.FunctionFlag, "Признак функционирования" },
            { Headers.AccessFlag, "Условия доступа" },
            { Headers.Password, "Пароль" },
            { Headers.LongitudeWGS84, "Долгота в WGS-84"},
            { Headers.LatitudeWGS84,  "Широта в WGS-84"},
            { Headers.GeodataCenter, "geodata_center"},
            { Headers.Geoarea, "geoarea" }
        };

        // Абстактные методы для представления объекта в форматах csv и json
        public abstract string ToCsv();
        public abstract string ToJson();
    }
}
