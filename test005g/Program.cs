using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

class VulnerabilityScanner
{
	private Dictionary<string, string> _signatures;
	private string _reportFile = "report.txt";
	private List<string> _filesToScan = new List<string> { "testfile1.exe", "testfile2.exe" }; // Укажите свои файлы

	public VulnerabilityScanner(string signatureFilePath)
	{
		_signatures = LoadSignatures(signatureFilePath);
	}

	private Dictionary<string, string> LoadSignatures(string path)
	{
		if (!File.Exists(path))
		{
			Console.WriteLine("Файл с сигнатурами не найден.");
			Environment.Exit(1);
		}

		string json = File.ReadAllText(path);
		return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
	}

	private string ComputeFileHash(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return string.Empty;
		}

		using (var sha256 = SHA256.Create())
		{
			using (var stream = File.OpenRead(filePath))
			{
				byte[] hashBytes = sha256.ComputeHash(stream);
				return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			}
		}
	}

	public void ScanAndSaveReport()
	{
		using (StreamWriter writer = new StreamWriter(_reportFile, false))
		{
			writer.WriteLine($"Отчет о проверке ({DateTime.Now}):");
			writer.WriteLine("-----------------------------------------");

			foreach (var file in _filesToScan)
			{
				if (!File.Exists(file))
				{
					writer.WriteLine($"Файл {file} не найден.");
					continue;
				}

				var foundVulnerabilities = ScanFile(file);
				if (foundVulnerabilities.Count == 0)
				{
					writer.WriteLine($"Файл {file} безопасен.");
				}
				else
				{
					writer.WriteLine($"Найдены уязвимости в {file}:");
					foreach (var vuln in foundVulnerabilities)
					{
						writer.WriteLine($" - {vuln}");
					}
				}
				writer.WriteLine();
			}
		}

		Console.WriteLine("Проверка завершена. Результат сохранен в report.txt.");
		System.Diagnostics.Process.Start("notepad.exe", _reportFile);
	}

	private List<string> ScanFile(string filePath)
	{
		string fileHash = ComputeFileHash(filePath);
		return _signatures
			.Where(sig => sig.Value.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
			.Select(sig => sig.Key)
			.ToList();
	}

	public static void Main()
	{
		string signatureFile = "signatures.json"; // Укажите путь к файлу сигнатур

		try
		{
			var scanner = new VulnerabilityScanner(signatureFile);
			scanner.ScanAndSaveReport();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка: {ex.Message}");
		}
	}
}
