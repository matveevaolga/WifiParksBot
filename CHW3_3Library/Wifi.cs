using System.Text.Json;
using System.Text.Json.Serialization;

namespace CHW3_3Library
{
    /// <summary>
    /// Generic-класс, наследуемый от абстрактного WifiObject.
    /// Представляет запись в wifi-parks файле.
    /// Реализует интерфейс IJsonOnDeserialized
    /// </summary>
    /// <typeparam name="I"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <typeparam name="Lo"></typeparam>
    /// <typeparam name="La"></typeparam>
    public class Wifi<I, C, Lo, La> : WifiObject, IJsonOnDeserialized
    {
        public I ID { get; }
        [JsonPropertyName("global_id")]
        public string GlobalID { get; }
        public string Name { get; }
        public string AdmArea { get; }
        public string District { get; }
        public string ParkName { get; }
        public string WifiName { get; }
        public C CoverageArea { get; }
        public string FunctionFlag { get; }
        public string AccessFlag { get; }
        public string Password { get; }
        [JsonPropertyName("Longitude_WGS84")]
        public Lo LongitudeWGS84 { get; }
        [JsonPropertyName("Latitude_WGS84")]
        public La LatitudeWGS84 { get; }
        [JsonPropertyName("geodata_center")]
        public string GeodataCenter { get; }
        public string Geoarea { get; }

        /// <summary>
        /// Конструктор, используемый для
        /// десериализации объекта Wifi
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="globalID"></param>
        /// <param name="name"></param>
        /// <param name="admArea"></param>
        /// <param name="district"></param>
        /// <param name="parkName"></param>
        /// <param name="wifiName"></param>
        /// <param name="coverageArea"></param>
        /// <param name="functionFlag"></param>
        /// <param name="accessFlag"></param>
        /// <param name="password"></param>
        /// <param name="longitudeWGS84"></param>
        /// <param name="latitudeWGS84"></param>
        /// <param name="geodataCenter"></param>
        /// <param name="geoarea"></param>
        [JsonConstructor]
        public Wifi(I iD, string globalID,
            string name, string admArea,
            string district, string parkName,
            string wifiName, C coverageArea,
            string functionFlag, string accessFlag,
            string password, Lo longitudeWGS84,
            La latitudeWGS84, string geodataCenter,
            string geoarea)
        {
            ID = iD;
            GlobalID = globalID;
            Name = name;
            AdmArea = admArea;
            District = district;
            ParkName = parkName;
            WifiName = wifiName;
            CoverageArea = coverageArea;
            FunctionFlag = functionFlag;
            AccessFlag = accessFlag;
            Password = password;
            LongitudeWGS84 = longitudeWGS84;
            LatitudeWGS84 = latitudeWGS84;
            GeodataCenter = geodataCenter;
            Geoarea = geoarea;
        }

        /// <summary>
        /// Конструктор, принимающий массив
        /// для заполнения полей объекта Wifi 
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        public Wifi(object[] values)
        {
            if (values.GetLength(0) != 15)
                throw new ArgumentException();
            ID = (I)Convert.ChangeType(values[0], typeof(I));
            GlobalID = (string)values[1];
            Name = (string)values[2];
            AdmArea = (string)values[3];
            District = (string)values[4];
            ParkName = (string)values[5];
            WifiName = (string)values[6];
            CoverageArea = (C)Convert.ChangeType(values[7], typeof(I));
            FunctionFlag = (string)values[8];
            AccessFlag = (string)values[9];
            Password = (string)values[10];
            LongitudeWGS84 = (Lo)Convert.ChangeType(values[11], typeof(Lo));
            LatitudeWGS84 = (La)Convert.ChangeType(values[12], typeof(La));
            GeodataCenter = (string)values[13];
            Geoarea = (string)values[14];
        }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Wifi()
        {
            ID = default;
            GlobalID = Name = AdmArea = District = ParkName = WifiName = "-";
            CoverageArea = default;
            FunctionFlag = AccessFlag = Password = "-";
            LongitudeWGS84 = default;
            LatitudeWGS84 = default;
            GeodataCenter = Geoarea = "-";
        }

        /// <summary>
        /// Метод, вызываемый после завершения сериализации.
        /// Проверяет корректное заполнение полей объекта Wifi
        /// </summary>
        /// <exception cref="JsonException"></exception>
        public void OnDeserialized()
        {
            if (new List<object> { ID, GlobalID, Name, AdmArea,
                District, ParkName, WifiName, CoverageArea, FunctionFlag,
                AccessFlag, Password, LongitudeWGS84, LatitudeWGS84, GeodataCenter,
                Geoarea}.Any(prop => prop == null))
                throw new JsonException("Записи в переданном файле не корректны");
        }

        /// <summary>
        /// Переписанный метод для записи
        /// представления Wifi объекта в csv файл
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public override string ToCsv()
        {
            return string.Format("\"{0}\";\"{1}\";\"{2}\";\"{3}\";\"{4}\";" +
               "\"{5}\";\"{6}\";\"{7}\";\"{8}\";\"{9}\";\"{10}\";\"{11}\";" +
               "\"{12}\";\"{13}\";\"{14}\";", ID, GlobalID,
                Name, AdmArea, District,
                ParkName, WifiName, CoverageArea,
                FunctionFlag, AccessFlag, Password,
                LongitudeWGS84, LatitudeWGS84, GeodataCenter,
                Geoarea);
        }

        /// <summary>
        /// Переписанный метод для записи
        /// представления Wifi объекта в json файл
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override string ToJson() => JsonSerializer.Serialize(this, options);
    }
}
