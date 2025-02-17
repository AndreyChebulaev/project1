using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

class SyscallMonitor
{
	static List<string> eventLog = new List<string>();

	static void Main()
	{
		Console.WriteLine("[+] Запуск мониторинга системных вызовов...");
		MonitorSystemCalls();
	}

	static void MonitorSystemCalls()
	{
		Process[] processes = Process.GetProcesses();

		foreach (var process in processes)
		{
			string logEntry = $"{DateTime.Now}: Запущен процесс {process.ProcessName} (PID: {process.Id})";
			Console.WriteLine(logEntry);
			eventLog.Add(logEntry);
		}

		SaveReport();
	}

	static void SaveReport()
	{
		string reportPath = "syscall_report.json";
		File.WriteAllText(reportPath, JsonSerializer.Serialize(eventLog, new JsonSerializerOptions { WriteIndented = true }));
		Console.WriteLine($"[+] Отчет сохранен: {reportPath}");
	}
}
