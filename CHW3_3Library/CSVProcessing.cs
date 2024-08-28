using System.Text;

namespace CHW3_3Library
{
    public class CSVProcessing
    {
        const string header = "\"ID\";\"global_id\";\"Name\";\"AdmArea\";\"District\";" +
                "\"ParkName\";\"WiFiName\";\"CoverageArea\";" +
                "\"FunctionFlag\";\"AccessFlag\";\"Password\";" +
                "\"Longitude_WGS84\";\"Latitude_WGS84\";\"geodata_center\";\"geoarea\";";

        /// <summary>
        /// Метод, принимающий объект WifiList и возвращающий поток с 
        /// его содержимым в формате csv
        /// </summary>
        /// <param name="wifiList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        public Stream Write(WifiList wifiList)
        {
            string text = header + "\n";
            foreach (WifiObject wifiObject in wifiList)
                text += wifiObject.ToCsv() + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(bytes);
            return stream;
        }

        /// <summary>
        /// Метод, принимающий поток с csv-представлением WifiList и
        /// возвращающий его в виде объекта класса WifiList
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public WifiList? Read(Stream stream)
        {
            var list = new List<Wifi<int, int, double, double>>();
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
            if (streamReader.ReadLine() != header)
                throw new FormatException();
            if (streamReader.ReadLine() != new WifiList().RussianHeaders.ToCsv())
                throw new FormatException();
            string? line;
            while ((line = streamReader.ReadLine()) != null)
                list.Add(FormWifi(line));
            streamReader.Close();
            return new WifiList(list);
        }

        /// <summary>
        /// Создание объекта Wifi на основе данных из переданной строки
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Wifi<int, int, double, double> FormWifi(string line)
        {
            object[] values = line.Split(';').Select(x => x.Trim().Trim('\"')).ToArray();
            if (values.GetLength(0) != header.Split(';').GetLength(0)) throw new FormatException();
            values[11] = values[11].ToString().Replace(".", ",");
            values[12] = values[12].ToString().Replace(".", ",");
            return new Wifi<int, int, double, double>(values[..^1]);
        }
    }
}
