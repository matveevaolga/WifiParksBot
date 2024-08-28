using CHW3_3Library;
using System.Text.Json;

namespace CHW3_3
{
    public class Program
    {
        async static Task Main(string[] args)
        {
            try
            {
                BotRunner botRunner = new BotRunner();
                await botRunner.Run();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("При создании объекта BotRunner " +
                " произошла непредвиденная ошибка ArgumentException");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"При создании объекта " +
                $" BotRunner произошла непредвиденная ошибка {ex.GetType().Name})");
            }
        }
    }
}