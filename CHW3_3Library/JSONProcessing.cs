using System.Text;
using System.Text.Json;

namespace CHW3_3Library
{
    public class JSONProcessing
    {
        /// <summary>
        /// Метод, принимающий объект WifiList и возвращающий поток с 
        /// его содержимым в формате json
        /// </summary>
        /// <param name="wifiList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        public Stream Write(WifiList wifiList)
        {
            string file = JsonSerializer.Serialize(wifiList, WifiObject.options);
            byte[] bytes = Encoding.UTF8.GetBytes(file);
            MemoryStream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Метод, принимающий поток с json-представлением WifiList и
        /// возвращающий его в виде объекта класса WifiList
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonException"></exception>
        public WifiList Read(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
            string file = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<WifiList>(file, WifiObject.options);
        }
    }
}
