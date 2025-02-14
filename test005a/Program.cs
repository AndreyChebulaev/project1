using System;
using System.Diagnostics;
using System.IO;

class SyscallAnalyzer
{
	static void Main(string[] args)
	{
		string logFilePath = "syscall_log.txt";
		RunProcessAndAnalyze("updated_software.exe", logFilePath);
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
				process.OutputDataReceived += (sender, e) => LogData(logFilePath, e.Data);
				process.ErrorDataReceived += (sender, e) => LogData(logFilePath, e.Data);

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка при запуске процесса: {ex.Message}");
		}
	}

	static void LogData(string filePath, string? data)
	{
		if (!string.IsNullOrEmpty(data))
		{
			File.AppendAllText(filePath, data + Environment.NewLine);
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
			if (line.Contains("error", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"Обнаружена ошибка: {line}");
			}
		}
	}
}

