using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class SecurityFileEncryptor
{
	static void Main(string[] args)
	{
		Console.WriteLine("Введите путь к файлу, который нужно зашифровать:");
		string inputFilePath = Console.ReadLine();

		Console.WriteLine("Введите путь для сохранения зашифрованного файла:");
		string encryptedFilePath = Console.ReadLine();

		Console.WriteLine("Введите ключ для шифрования:");
		string key = Console.ReadLine();

		try
		{
			EncryptFile(inputFilePath, encryptedFilePath, key);
			Console.WriteLine("Файл успешно зашифрован и сохранён по адресу: " + encryptedFilePath);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Произошла ошибка: " + ex.Message);
		}
	}

	public static void EncryptFile(string inputFile, string outputFile, string key)
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
			using (CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				inputStream.CopyTo(cryptoStream);
			}
		}
	}
}
