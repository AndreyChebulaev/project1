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
			Console.WriteLine("Security Update Analyzer");

			// Путь к обновлению безопасности
			Console.Write("Enter path to the update file: ");
			string updateFilePath = Console.ReadLine();

			if (!File.Exists(updateFilePath))
			{
				Console.WriteLine("File not found.");
				return;
			}

			// Загрузка индикаторов компрометации (IoC)
			var iocs = LoadIoCs("ioc_list.txt");

			// Проверка на IoC
			bool hasIoCs = AnalyzeForIoCs(updateFilePath, iocs);
			if (hasIoCs)
			{
				Console.WriteLine("Potential IoCs found in the update file.");
			}
			else
			{
				Console.WriteLine("No IoCs detected in the update file.");
			}

			// Проверка с использованием YARA-правил
			bool hasYaraMatches = AnalyzeWithYara(updateFilePath);
			if (hasYaraMatches)
			{
				Console.WriteLine("Potential YARA rule matches found.");
			}
			else
			{
				Console.WriteLine("No YARA rule matches detected.");
			}
		}

		static List<string> LoadIoCs(string iocFilePath)
		{
			var iocs = new List<string>();

			if (!File.Exists(iocFilePath))
			{
				Console.WriteLine("IoC file not found. Continuing without IoCs.");
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

		static bool AnalyzeForIoCs(string filePath, List<string> iocs)
		{
			string fileContent = File.ReadAllText(filePath);

			foreach (var ioc in iocs)
			{
				if (Regex.IsMatch(fileContent, ioc, RegexOptions.IgnoreCase))
				{
					Console.WriteLine($"IoC detected: {ioc}");
					return true;
				}
			}

			return false;
		}

		static bool AnalyzeWithYara(string filePath)
		{
			try
			{
				string yaraExecutable = "yara"; // YARA CLI executable
				string yaraRulesFile = "rules.yar"; // YARA rules file

				if (!File.Exists(yaraRulesFile))
				{
					Console.WriteLine("YARA rules file not found.");
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
						Console.WriteLine("YARA output:\n" + output);
						return true;
					}

					if (!string.IsNullOrEmpty(error))
					{
						Console.WriteLine("YARA error:\n" + error);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running YARA: {ex.Message}");
			}

			return false;
		}
	}
}
