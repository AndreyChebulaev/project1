using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SecurityAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Анализатор обновлений безопасности (Тест T004)");

			// Путь к файлу обновления безопасности
			Console.Write("Введите путь к файлу обновления: ");
			string updateFilePath = Console.ReadLine();

			if (!File.Exists(updateFilePath))
			{
				Console.WriteLine("Файл не найден. Пожалуйста, проверьте путь и попробуйте снова.");
				return;
			}

			string logFilePath = "analysis_results.txt";
			using (StreamWriter logFile = new StreamWriter(logFilePath, false))
			{
				logFile.WriteLine("Результаты анализа безопасности (Тест T004)");
				logFile.WriteLine($"Анализируемый файл: {updateFilePath}\n");

				// Загрузка индикаторов компрометации (IoC)
				var iocs = LoadIoCs("ioc_list.txt");

				// Анализ на наличие IoC
				bool hasIoCs = AnalyzeForIoCs(updateFilePath, iocs, logFile);
				if (hasIoCs)
				{
					logFile.WriteLine("В файле обновления найдены возможные индикаторы компрометации (IoC).\n");
				}
				else
				{
					logFile.WriteLine("Индикаторы компрометации (IoC) не обнаружены в файле обновления.\n");
				}

				// Анализ с использованием YARA-правил
				bool hasYaraMatches = AnalyzeWithYara(updateFilePath, logFile);
				if (hasYaraMatches)
				{
					logFile.WriteLine("Обнаружены возможные совпадения с YARA-правилами.\n");
				}
				else
				{
					logFile.WriteLine("Совпадений с YARA-правилами не обнаружено.\n");
				}

				// Контекстный поиск запрещенной информации
				bool hasForbiddenKeywords = AnalyzeForKeywords(updateFilePath, logFile);
				if (hasForbiddenKeywords)
				{
					logFile.WriteLine("В файле обновления найдены запрещенные ключевые слова.\n");
				}
				else
				{
					logFile.WriteLine("Запрещенные ключевые слова не обнаружены в файле обновления.\n");
				}

				Console.WriteLine($"Результаты анализа сохранены в файл: {logFilePath}");
			}
		}

		// Метод для загрузки индикаторов компрометации (IoC) из файла
		static List<string> LoadIoCs(string iocFilePath)
		{
			var iocs = new List<string>();

			if (!File.Exists(iocFilePath))
			{
				Console.WriteLine("Файл с IoC не найден. Продолжаем без проверки на IoC.");
				return iocs;
			}

			using (var reader = new StreamReader(iocFilePath))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						iocs.Add(line.Trim());
					}
				}
			}

			return iocs;
		}

		// Метод для проверки файла на наличие индикаторов компрометации (IoC)
		static bool AnalyzeForIoCs(string filePath, List<string> iocs, StreamWriter logFile)
		{
			string fileContent = File.ReadAllText(filePath);

			foreach (var ioc in iocs)
			{
				if (Regex.IsMatch(fileContent, ioc, RegexOptions.IgnoreCase))
				{
					logFile.WriteLine($"Обнаружен IoC: {ioc}");
					return true;
				}
			}

			return false;
		}

		// Метод для анализа файла с использованием YARA-правил
		static bool AnalyzeWithYara(string filePath, StreamWriter logFile)
		{
			try
			{
				string yaraExecutable = "yara"; // Исполняемый файл YARA CLI
				string yaraRulesFile = "rules.yar"; // Файл с YARA-правилами

				if (!File.Exists(yaraRulesFile))
				{
					logFile.WriteLine("Файл с YARA-правилами не найден.");
					return false;
				}

				var processStartInfo = new ProcessStartInfo
				{
					FileName = yaraExecutable,
					Arguments = $"{yaraRulesFile} {filePath}",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (var process = Process.Start(processStartInfo))
				{
					string output = process.StandardOutput.ReadToEnd();
					string error = process.StandardError.ReadToEnd();

					process.WaitForExit();

					if (!string.IsNullOrEmpty(output))
					{
						logFile.WriteLine("Результат YARA:");
						logFile.WriteLine(output);
						return true;
					}

					if (!string.IsNullOrEmpty(error))
					{
						logFile.WriteLine("Ошибка YARA:");
						logFile.WriteLine(error);
					}
				}
			}
			catch (Exception ex)
			{
				logFile.WriteLine($"Исключение при выполнении YARA: {ex.Message}");
			}

			return false;
		}

		// Метод для поиска запрещенных ключевых слов в файле
		static bool AnalyzeForKeywords(string filePath, StreamWriter logFile)
		{
			string fileContent = File.ReadAllText(filePath);
			string[] keywords = { "политика", "баннер", "лозунг", "противоправный" };
			bool keywordFound = false;

			foreach (var keyword in keywords)
			{
				MatchCollection matches = Regex.Matches(fileContent, keyword, RegexOptions.IgnoreCase);
				if (matches.Count > 0)
				{
					logFile.WriteLine($"Обнаружено запрещенное ключевое слово: {keyword}");
					keywordFound = true;
				}
			}

			return keywordFound;
		}
	}
}
