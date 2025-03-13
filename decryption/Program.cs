using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class SecurityFileDecryptor
{
	static void Main(string[] args)
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

	public static void DecryptFile(string inputFile, string outputFile, string key)
	{
		// Преобразование ключа в байты
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		using (Aes aes = Aes.Create())
		{
			// Используем фиксированный IV для демонстрации (лучше сохранять IV рядом с зашифрованным файлом)
			byte[] iv = new byte[16];
			Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));

			aes.Key = keyBytes;
			aes.IV = iv;

			// Чтение зашифрованного файла
			using (FileStream inputStream = new FileStream(inputFile, FileMode.Open))
			using (FileStream outputStream = new FileStream(outputFile, FileMode.Create))
			using (CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
			{
				cryptoStream.CopyTo(outputStream);
			}
		}
	}
}
