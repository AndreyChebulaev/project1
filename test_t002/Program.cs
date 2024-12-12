using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class SecurityUpdateVerifier
{
	static void Main(string[] args)
	{
		Console.WriteLine("Выберите действие:");
		Console.WriteLine("1. Расшифровать файл обновления");
		Console.WriteLine("2. Проверить подлинность файла обновления");
		Console.Write("Введите номер действия: ");
		string choice = Console.ReadLine();

		switch (choice)
		{
			case "1":
				DecryptFileWorkflow();
				break;
			case "2":
				VerifyUpdateFileWorkflow();
				break;
			default:
				Console.WriteLine("Неверный выбор.");
				break;
		}
	}

	static void DecryptFileWorkflow()
	{
		Console.WriteLine("Введите путь к зашифрованному файлу:");
		string encryptedFilePath = Console.ReadLine();

		Console.WriteLine("Введите путь для сохранения расшифрованного файла:");
		string decryptedFilePath = Console.ReadLine();

		Console.WriteLine("Введите ключ для расшифрования:");
		string key = Console.ReadLine();

		try
		{
			DecryptFile(encryptedFilePath, decryptedFilePath, key);
			Console.WriteLine("Файл успешно расшифрован и сохранён по адресу: " + decryptedFilePath);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Произошла ошибка: " + ex.Message);
		}
	}

	static void VerifyUpdateFileWorkflow()
	{
		Console.WriteLine("Введите путь к файлу обновления безопасности:");
		string filePath = Console.ReadLine();

		string resultsFilePath = "verification_results.txt";

		if (!File.Exists(filePath))
		{
			Console.WriteLine("Файл обновления безопасности не найден.");
			return;
		}

		// Эталонная контрольная сумма
		string referenceChecksum;
		if (!File.Exists(resultsFilePath))
		{
			Console.WriteLine("Эталонная контрольная сумма будет создана.");
			referenceChecksum = GetFileChecksum(filePath);
			File.WriteAllText(resultsFilePath, $"Эталонная контрольная сумма: {referenceChecksum}\n");
			Console.WriteLine("Эталонная контрольная сумма сохранена.");
		}
		else
		{
			referenceChecksum = File.ReadAllText(resultsFilePath)
									.Split("\n")[0]
									.Replace("Эталонная контрольная сумма: ", "")
									.Trim();
		}

		// Проверяемая контрольная сумма
		string currentChecksum = GetFileChecksum(filePath);

		// Сравнение и запись результатов
		bool isValid = referenceChecksum.Equals(currentChecksum, StringComparison.OrdinalIgnoreCase);

		using (StreamWriter writer = new StreamWriter(resultsFilePath, true))
		{
			writer.WriteLine($"Проверяемая контрольная сумма: {currentChecksum}");
			writer.WriteLine($"Результат проверки: {(isValid ? "Совпадает" : "Не совпадает")}");
			writer.WriteLine(new string('-', 40));
		}

		Console.WriteLine($"Результаты сохранены в файл: {resultsFilePath}");
	}

	static void DecryptFile(string inputFile, string outputFile, string key)
	{
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		using (Aes aes = Aes.Create())	
		{
			byte[] iv = new byte[16];
			Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));

			aes.Key = keyBytes;
			aes.IV = iv;

			using (FileStream inputStream = new FileStream(inputFile, FileMode.Open))
			using (FileStream outputStream = new FileStream(outputFile, FileMode.Create))
			using (CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
			{
				cryptoStream.CopyTo(outputStream);
			}
		}
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
