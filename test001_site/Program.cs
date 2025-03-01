using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

class SecurityUpdateChecker
{
	// Метод выполняет проверку обновлений безопасности, загружая файлы и вычисляя их контрольные суммы
	public async Task<(string checksum1, string checksum2, bool areIdentical)> RunSecurityUpdateCheck(string url1, string url2, string savePath1, string savePath2)
	{
		// Загружаем файлы по переданным URL
		await DownloadFileAsync(url1, savePath1);
		await DownloadFileAsync(url2, savePath2);

		// Вычисляем контрольные суммы загруженных файлов
		string checksum1 = CalculateChecksum(savePath1);
		string checksum2 = CalculateChecksum(savePath2);

		// Возвращаем контрольные суммы и результат сравнения
		return (checksum1, checksum2, checksum1 == checksum2);
	}

	// Метод загружает файл по HTTP или FTP
	private async Task DownloadFileAsync(string url, string savePath)
	{
		if (url.StartsWith("http")) // Если URL начинается с http, используем HTTP-клиент
		{
			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode(); // Проверяем, что запрос успешен
				string content = await response.Content.ReadAsStringAsync();
				await File.WriteAllTextAsync(savePath, content); // Сохраняем файл
			}
		}
		else if (url.StartsWith("ftp")) // Если URL начинается с ftp, используем FTP-запрос
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			using (StreamWriter writer = new StreamWriter(savePath))
			{
				writer.Write(reader.ReadToEnd()); // Сохраняем файл
			}
		}
		else
		{
			throw new ArgumentException("Unsupported URL format."); // Бросаем исключение при неподдерживаемом формате URL
		}
	}

	// Метод вычисляет контрольную сумму файла с использованием SHA-256
	private string CalculateChecksum(string filePath)
	{
		using (SHA256 sha256 = SHA256.Create())
		using (FileStream fs = File.OpenRead(filePath))
		{
			byte[] hashBytes = sha256.ComputeHash(fs);
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashBytes)
			{
				sb.Append(b.ToString("x2")); // Преобразуем байты в строку в 16-ричном формате
			}
			return sb.ToString(); // Возвращаем контрольную сумму
		}
	}
}
