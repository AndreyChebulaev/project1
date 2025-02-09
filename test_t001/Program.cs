using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//программа для загрузки обновлений безопасности из различных источников(http,ftp) и используя российский и иностранный адресс(прокси)
//Наконец, код записывает вычисленные контрольные суммы в файл "checksums_comparison.txt". Каждая строка в файле содержит путь к файлу и соответствующую контрольную сумму.
class Program
{
	static async Task Main(string[] args)
	{
		// URL для загрузки файлов
		string httpUrl = "https://haveibeenpwned.com/api/v3/breaches";
		string ftpUrl = "ftp://ftp.ncbi.nlm.nih.gov/pub/README.ftp";
		string proxyUrl = "https://haveibeenpwned.com/api/v3/breaches";
		string fileExtensionHttp = Path.GetExtension(httpUrl);
		string fileExtensionFtp = Path.GetExtension(ftpUrl);
		string fileExtensionProxy = Path.GetExtension(proxyUrl);
		string savePathHttp = "update_http" + fileExtensionHttp;
		string savePathFtp = "update_ftp" + fileExtensionFtp;
		string savePathProxyRus = "update_proxy_rus" + fileExtensionProxy;
		string savePathProxyEng = "update_proxy_eng" + fileExtensionProxy;
		string checksumsFilePath = "checksums_comparison.txt";

		// Загрузка файлов с HTTP и FTP
		await DownloadFileHttpAsync(httpUrl, savePathHttp);
		DownloadFileFtp(ftpUrl, savePathFtp);

		// Прокси-серверы для российских и иностранных IP-адресов
		var russianProxy = new WebProxy("89.250.152.76");
		var foreignProxy = new WebProxy("103.69.20.41");

		// Скачивание через прокси
		await DownloadFileWithProxy(proxyUrl, savePathProxyRus, russianProxy);
		await DownloadFileWithProxy(proxyUrl, savePathProxyEng, foreignProxy);

		// Подсчет и запись контрольных сумм
		string[] filePaths = { savePathHttp, savePathFtp, savePathProxyRus, savePathProxyEng };
		using (StreamWriter writer = new StreamWriter(checksumsFilePath))
		{
			foreach (string filePath in filePaths)
			{
				string checksum = CalculateChecksum(filePath);
				writer.WriteLine($"{filePath}: {checksum}");
			}
		}

		Console.WriteLine($"Контрольные суммы записаны в файл '{checksumsFilePath}'");
	}

	//Метод для вычисления контрольной суммы
	//SHA-256 (Secure Hash Algorithm 256) - это криптографическая хеш-функция из семейства алгоритмов SHA-2, разработанная Национальным институтом стандартов и технологий США  (NIST).
	//Вычисляется хеш-сумма содержимого файла с помощью SHA256 . Метод читает байты из файлового потока fs и возвращает вычисленный хеш в виде массива байтов (hashBytes).
	//Далее, каждый байт хеша преобразуется в шестнадцатеричное представление с помощью b.ToString("x2") и добавляется в StringBuilder
	//В результате получается строковое представление контрольной суммы файла.
	//Строковое представление контрольной суммы возвращается методом CalculateChecksum и записывается в файл вместе с путем к соответствующему файлу.

	static string CalculateChecksum(string filePath)
	{
		using (SHA256 sha256 = SHA256.Create())
		using (FileStream fs = File.OpenRead(filePath))
		{
			byte[] hashBytes = sha256.ComputeHash(fs);
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashBytes)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString();
		}
	}

	// Метод для загрузки файла через HTTP
	static async Task DownloadFileHttpAsync(string url, string savePath)
	{
		using (HttpClient client = new HttpClient())
		{
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();
			string content = await response.Content.ReadAsStringAsync();
			await File.WriteAllTextAsync(savePath, content);
		}
	}

	// Метод для загрузки файла через FTP
	static void DownloadFileFtp(string url, string savePath)
	{
		FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
		request.Method = WebRequestMethods.Ftp.DownloadFile;

		using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
		using (StreamReader reader = new StreamReader(response.GetResponseStream()))
		using (StreamWriter writer = new StreamWriter(savePath))
		{
			writer.Write(reader.ReadToEnd());
		}
	}

	// Метод для скачивания файла с использованием прокси
	static async Task DownloadFileWithProxy(string url, string filePath, IWebProxy proxy)
	{
		var httpClientHandler = new HttpClientHandler { Proxy = proxy, UseProxy = true }; //fasle если без прокси

		using (var httpClient = new HttpClient(httpClientHandler))
		{
			try
			{
				var response = await httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					await response.Content.CopyToAsync(fileStream);
				}

				Console.WriteLine($"Файл успешно скачан и сохранен в {filePath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при скачивании файла: {ex.Message}");
			}
		}
	}
}
