using System;
using System.Diagnostics;
using System.IO;

class SyscallAnalyzer
{
	static void Main(string[] args)
	{
		string logFilePath = "syscall_log.txt";
		string executablePath = "updated_software.exe";

		if (!File.Exists(executablePath))
		{
			Console.WriteLine($"Ошибка: файл {executablePath} не найден.");
			return;
		}

		RunProcessAndAnalyze(executablePath, logFilePath);
		AnalyzeLog(logFilePath);
	}

	static void RunProcessAndAnalyze(string executable, string logFilePath)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = executable,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (Process process = new Process { StartInfo = startInfo })
			{
				using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
				{
					process.OutputDataReceived += (sender, e) => LogData(writer, e.Data);
					process.ErrorDataReceived += (sender, e) => LogData(writer, e.Data);

					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка при запуске процесса: {ex.Message}");
		}
	}

	static void LogData(StreamWriter writer, string? data)
	{
		if (!string.IsNullOrEmpty(data))
		{
			writer.WriteLine(data);
		}
	}

	static void AnalyzeLog(string logFilePath)
	{
		if (!File.Exists(logFilePath))
		{
			Console.WriteLine("Файл логов отсутствует.");
			return;
		}

		string[] logLines = File.ReadAllLines(logFilePath);
		foreach (string line in logLines)
		{
			if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
				line.Contains("fail", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"Обнаружена ошибка: {line}");
			}
		}
	}
}

