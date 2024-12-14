using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Введите путь к файлу:");
        string filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден. Пожалуйста, проверьте путь и попробуйте снова.");
            return;
        }

        string text = File.ReadAllText(filePath);
        string[] keywords = { "политика", "баннер", "лозунг", "противоправный" };
        bool keywordFound = false;

        foreach (var keyword in keywords)
        {
            MatchCollection matches = Regex.Matches(text, keyword, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                Console.WriteLine($"Найдено ключевое слово: {keyword}");
                keywordFound = true;
            }
        }

        if (!keywordFound)
        {
            Console.WriteLine("Запрещенные слова не найдены.");
        }
    }
}