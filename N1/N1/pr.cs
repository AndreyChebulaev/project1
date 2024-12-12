﻿using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        // Путь к файлу обновления безопасности
        string filePath = "update.bin";

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

    // Метод для получения контрольной суммы файла (SHA256)
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