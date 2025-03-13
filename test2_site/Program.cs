using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class SecurityUpdateVerifier
{
	// Метод для расшифровки файла с использованием ключа
	public static bool DecryptFile(string inputFile, string outputFile, string key)
	{
		try
		{
			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			using (Aes aes = Aes.Create())
			{
				// Инициализация вектора IV на основе ключа
				byte[] iv = new byte[16];
				Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));

				aes.Key = keyBytes;
				aes.IV = iv;

				// Открываем входной и выходной файлы, создаем поток для расшифровки
				using (FileStream inputStream = new FileStream(inputFile, FileMode.Open))
				using (FileStream outputStream = new FileStream(outputFile, FileMode.Create))
				using (CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
				{
					cryptoStream.CopyTo(outputStream); // Копируем расшифрованные данные в выходной файл
				}
			}
			return true; // Успешная расшифровка
		}
		catch
		{
			return false; // Ошибка при расшифровке.
		}
	}

	// Метод для проверки целостности файла обновления безопасности
	public static (string referenceChecksum, string currentChecksum, bool isValid, string resultsFilePath) VerifyUpdateFile(string filePath, string resultsFilePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException("Файл обновления безопасности не найден.");
		}

		string referenceChecksum;

		// Проверяем, существует ли эталонная контрольная сумма
		if (!File.Exists(resultsFilePath))
		{
			referenceChecksum = GetFileChecksum(filePath);
			File.WriteAllText(resultsFilePath, $"Эталонная контрольная сумма: {referenceChecksum}\n");
		}
		else
		{
			// Читаем ранее сохраненную эталонную контрольную сумму
			referenceChecksum = File.ReadAllText(resultsFilePath)
									.Split("\n")[0]
									.Replace("Эталонная контрольная сумма: ", "")
									.Trim();
		}

		// Вычисляем контрольную сумму текущего файла
		string currentChecksum = GetFileChecksum(filePath);

		// Сравниваем эталонную и текущую контрольные суммы
		bool isValid = referenceChecksum.Equals(currentChecksum, StringComparison.OrdinalIgnoreCase);

		// Записываем результаты проверки в файл
		File.AppendAllText(resultsFilePath, $"Проверяемая контрольная сумма: {currentChecksum}\nРезультат проверки: {(isValid ? "Совпадает" : "Не совпадает")}\n{new string('-', 40)}\n");

		return (referenceChecksum, currentChecksum, isValid, resultsFilePath);
	}

	// Метод для вычисления контрольной суммы SHA-256 файла
	private static string GetFileChecksum(string filePath)
	{
		using (FileStream stream = File.OpenRead(filePath))
		{
			SHA256 sha256 = SHA256.Create();
			byte[] hash = sha256.ComputeHash(stream);
			return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
		}
	}
}
