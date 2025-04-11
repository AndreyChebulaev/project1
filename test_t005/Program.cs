using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UnifiedSecurityTest
{
	class Program
	{
		static string reportPath = "report.txt";

		static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			using StreamWriter writer = new StreamWriter(reportPath, false, Encoding.UTF8);

			writer.WriteLine("=== Объединённый отчёт по тесту Т005 ===");
			writer.WriteLine($"Дата и время: {DateTime.Now}");
			writer.WriteLine("----------------------------------------\n");

			Console.WriteLine("1. Анализ системных вызовов...");
			writer.WriteLine("1. Анализ системных вызовов (запущенные процессы):");
			SyscallMonitor.Run(writer);

			Console.WriteLine("2. Сравнение файловой системы...");
			writer.WriteLine("\n2. Анализ файловой системы до и после обновления:");
			FileSystemAnalyzer.Run(writer);

			Console.WriteLine("3. Сигнатурный анализ уязвимостей...");
			writer.WriteLine("\n3. Сигнатурный анализ уязвимостей:");
			VulnerabilityScanner.Run(writer);

			writer.WriteLine("\n=== Конец отчёта ===");

			Console.WriteLine($"\nПроверка завершена. Отчёт сохранён в файл {reportPath}.");
			Process.Start("notepad.exe", reportPath);
		}
	}

	class SyscallMonitor
	{
		public static void Run(StreamWriter writer)
		{
			var processes = Process.GetProcesses();

			foreach (var process in processes)
			{
				string logEntry = $"{DateTime.Now}: Запущен процесс {process.ProcessName} (PID: {process.Id})";
				writer.WriteLine(logEntry);
			}

			writer.WriteLine("[+] Мониторинг завершен.\n");
		}
	}

	class FileSystemAnalyzer
	{
		public class FileSystemEntry
		{
			public string Path { get; set; }
			public bool IsDirectory { get; set; }
			public long Size { get; set; }

			public FileSystemEntry(string path, bool isDirectory, long size = 0)
			{
				Path = path;
				IsDirectory = isDirectory;
				Size = size;
			}

			public override string ToString()
			{
				return $"{(IsDirectory ? "Каталог" : "Файл")}: {Path}{(Size > 0 ? $" ({Size} байт)" : "")}";
			}

			public override bool Equals(object obj)
			{
				return obj is FileSystemEntry other &&
					   Path == other.Path &&
					   IsDirectory == other.IsDirectory &&
					   Size == other.Size;
			}

			public override int GetHashCode()
			{
				return Path.GetHashCode() ^ IsDirectory.GetHashCode() ^ Size.GetHashCode();
			}
		}

		public static void Run(StreamWriter writer)
		{
			Console.Write("Введите путь к исходному каталогу: ");
			string beforePath = Console.ReadLine();
			Console.Write("Введите путь к обновлённому каталогу: ");
			string afterPath = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(beforePath) || string.IsNullOrWhiteSpace(afterPath))
			{
				writer.WriteLine("Ошибка: Пути к каталогам не заданы.\n");
				return;
			}

			var before = GetFileSystemEntries(beforePath);
			var after = GetFileSystemEntries(afterPath);

			CompareFileSystems("Сравнение файловых систем", before, after, writer);
		}

		private static List<FileSystemEntry> GetFileSystemEntries(string path)
		{
			var entries = new List<FileSystemEntry>();

			try
			{
				foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
				{
					try
					{
						var info = new FileInfo(file);
						entries.Add(new FileSystemEntry(file, false, info.Length));
					}
					catch { }
				}

				foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
				{
					entries.Add(new FileSystemEntry(dir, true));
				}
			}
			catch (Exception ex)
			{
				entries.Add(new FileSystemEntry($"[Ошибка доступа: {ex.Message}]", true));
			}

			return entries;
		}

		private static void CompareFileSystems(string description, List<FileSystemEntry> before, List<FileSystemEntry> after, StreamWriter writer)
		{
			writer.WriteLine($"\n--- {description} ---");

			var added = after.Except(before).ToList();
			var removed = before.Except(after).ToList();

			writer.WriteLine("\nДобавленные элементы:");
			if (added.Count == 0) writer.WriteLine(" - Нет");
			added.ForEach(e => writer.WriteLine(" + " + e));

			writer.WriteLine("\nУдалённые элементы:");
			if (removed.Count == 0) writer.WriteLine(" - Нет");
			removed.ForEach(e => writer.WriteLine(" - " + e));

			writer.WriteLine("\nИзменённые по размеру файлы:");
			var changed = before.Where(b => !b.IsDirectory)
				.Select(b => (before: b, after: after.FirstOrDefault(a => a.Path == b.Path)))
				.Where(t => t.after != null && t.before.Size != t.after.Size);

			foreach (var entry in changed)
			{
				writer.WriteLine($" * {entry.before.Path}: {entry.before.Size} → {entry.after.Size} байт");
			}

			if (!changed.Any())
				writer.WriteLine(" - Нет изменений по размеру");

			writer.WriteLine("--- Конец сравнения ---\n");
		}
	}

	class VulnerabilityScanner
	{
		private static string SignaturePath = "signatures.json";
		private static List<string> FilesToScan = new List<string> { "testfile1.exe", "testfile2.exe" };

		public static void Run(StreamWriter writer)
		{
			if (!File.Exists(SignaturePath))
			{
				writer.WriteLine("Файл с сигнатурами не найден.");
				return;
			}

			var signatures = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SignaturePath));

			foreach (var file in FilesToScan)
			{
				if (!File.Exists(file))
				{
					writer.WriteLine($"Файл {file} не найден.");
					continue;
				}

				string hash = ComputeFileHash(file);
				var found = signatures.Where(s => s.Value.Equals(hash, StringComparison.OrdinalIgnoreCase))
									  .Select(s => s.Key).ToList();

				if (found.Count == 0)
				{
					writer.WriteLine($"Файл {file} безопасен.");
				}
				else
				{
					writer.WriteLine($"Найдены уязвимости в {file}:");
					found.ForEach(vuln => writer.WriteLine($" - {vuln}"));
				}

				writer.WriteLine();
			}
		}

		private static string ComputeFileHash(string filePath)
		{
			using var sha256 = SHA256.Create();
			using var stream = File.OpenRead(filePath);
			return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLower();
		}
	}
}
