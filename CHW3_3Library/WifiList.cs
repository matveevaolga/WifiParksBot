using System.Collections;
using System.Text.Json.Serialization;



namespace CHW3_3Library
{
    /// <summary>
    /// Класс, представляющий коллекцию объектов Wifi.
    /// Поддерживает итерацию, реализует IEnumerable
    /// </summary>
    public class WifiList
    {
        /// <summary>
        /// Объект Wifi, представляющий русские наименования заголовков файла wifi-parks.
        /// Есть в каждом созданным программой json-файле
        /// </summary>
        [JsonPropertyName("russian_naming")]
        public Wifi<string, string, string, string> RussianHeaders { get; } =
            new Wifi<string, string, string, string>(WifiObject.russianHeaders.Values.ToArray());

        List<Wifi<int, int, double, double>> wifis;
        public List<Wifi<int, int, double, double>> Wifis
        {
            get => wifis;
            set => wifis = value;
        }

        /// <summary>
        /// Конструктор, используемый для
        /// десериализации объекта WifiList
        /// </summary>
        /// <param name="wifis"></param>
        /// <exception cref="NullReferenceException"></exception>
        [JsonConstructor]
        public WifiList(List<Wifi<int, int, double, double>> wifis)
        {
            if (wifis == null) throw new NullReferenceException();
            this.wifis = wifis;
        }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public WifiList() => wifis = new List<Wifi<int, int, double, double>> { };

        /// <summary>
        /// Возвращает итератор по объекту WifiList
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            yield return RussianHeaders;
            foreach (var wifi in Wifis)
                yield return wifi;
        }

        /// <summary>
        /// Сортировка wifis
        /// </summary>
        /// <param name="header"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Sort(WifiObject.Headers header)
        {
            switch (header)
            {
                case WifiObject.Headers.Name:
                    wifis = (from wifi in wifis
                             orderby wifi.Name
                             ascending
                             select wifi).ToList();
                    break;
                case WifiObject.Headers.CoverageArea:
                    wifis = (from wifi in wifis
                             orderby wifi.CoverageArea
                             ascending
                             select wifi).ToList();
                    break;
            }
        }

        /// <summary>
        /// Выборка из wifis
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Select(WifiObject.Headers header, object value)
        {
            switch (header)
            {
                case WifiObject.Headers.CoverageArea:
                    wifis = (from wifi in wifis
                             where
                             wifi.CoverageArea.ToString()
                             == (string)value
                             select wifi).ToList();
                    break;
                case WifiObject.Headers.ParkName:
                    wifis = (from wifi in wifis
                             where
                             wifi.ParkName == (string)value
                             select wifi).ToList();
                    break;
                case WifiObject.Headers.AdmArea:
                    wifis = (from wifi in wifis
                             where
                             wifi.AdmArea == (string)value
                             select wifi).ToList();
                    break;
            }
        }        
    }
}
