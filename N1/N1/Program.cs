using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Пользователь загружает файл обновления безопасности
        Console.WriteLine("Введите путь к файлу обновления безопасности:");
        string filePath = Console.ReadLine();

        // Генерация эталонной контрольной суммы (это шаг может быть выполнен отдельно заранее)
        GenerateChecksum(filePath);

        // Проверка подлинности файла
        VerifyUpdateFile(filePath);

        // Ожидание нажатия клавиши перед закрытием консоли
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    static void GenerateChecksum(string filePath)
    {
        // Проверка существования файла
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден: " + filePath);
            return;
        }

        // Получение контрольной суммы файла
        string checksum = GetFileChecksum(filePath);
        Console.WriteLine("Эталонная контрольная сумма файла (SHA256): " + checksum);

        // Сохранение контрольной суммы в файл
        File.WriteAllText("reference_checksum.txt", checksum);
        Console.WriteLine("Эталонная контрольная сумма сохранена в reference_checksum.txt");
    }

    static void VerifyUpdateFile(string filePath)
    {
        // Путь к файлу с эталонной контрольной суммой
        string checksumFilePath = "reference_checksum.txt";

        // Проверка наличия файлов
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл обновления безопасности не найден: " + filePath);
            return;
        }

        if (!File.Exists(checksumFilePath))
        {
            Console.WriteLine("Файл с эталонной контрольной суммой не найден: " + checksumFilePath);
            return;
        }

        // Получение контрольной суммы файла
        string checksum = GetFileChecksum(filePath);
        Console.WriteLine("Контрольная сумма файла (SHA256): " + checksum);

        // Чтение эталонной контрольной суммы из файла
        string referenceChecksum = File.ReadAllText(checksumFilePath).Trim();
        Console.WriteLine("Эталонная контрольная сумма: " + referenceChecksum);

        // Сравнение контрольных сумм
        bool isValid = checksum.Equals(referenceChecksum, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine("Контрольная сумма действительна: " + isValid);
    }

    static string GetFileChecksum(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}