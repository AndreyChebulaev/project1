using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class SecurityUpdateService
{
	public async Task<string> ProcessUpdatesAsync()
	{
		string httpUrl = "https://haveibeenpwned.com/api/v3/breaches";
		string ftpUrl = "ftp://ftp.ncbi.nlm.nih.gov/pub/README.ftp";
		string proxyUrl = "https://haveibeenpwned.com/api/v3/breaches";

		string savePathHttp = Path.GetTempFileName();
		string savePathFtp = Path.GetTempFileName();
		string savePathProxyRus = Path.GetTempFileName();
		string savePathProxyEng = Path.GetTempFileName();

		await DownloadFileHttpAsync(httpUrl, savePathHttp);
		DownloadFileFtp(ftpUrl, savePathFtp);

		var russianProxy = new WebProxy("89.250.152.76");
		var foreignProxy = new WebProxy("103.69.20.41");

		await DownloadFileWithProxy(proxyUrl, savePathProxyRus, russianProxy);
		await DownloadFileWithProxy(proxyUrl, savePathProxyEng, foreignProxy);

		string[] filePaths = { savePathHttp, savePathFtp, savePathProxyRus, savePathProxyEng };
		StringBuilder report = new StringBuilder();

		foreach (string filePath in filePaths)
		{
			string checksum = CalculateChecksum(filePath);
			report.AppendLine($"{filePath}: {checksum}");
		}

		return report.ToString();
	}

	private static string CalculateChecksum(string filePath)
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

	private static async Task DownloadFileHttpAsync(string url, string savePath)
	{
		using (HttpClient client = new HttpClient())
		{
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();
			string content = await response.Content.ReadAsStringAsync();
			await File.WriteAllTextAsync(savePath, content);
		}
	}

	private static void DownloadFileFtp(string url, string savePath)
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

	private static async Task DownloadFileWithProxy(string url, string filePath, IWebProxy proxy)
	{
		var httpClientHandler = new HttpClientHandler { Proxy = proxy, UseProxy = true };

		using (var httpClient = new HttpClient(httpClientHandler))
		{
			var response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await response.Content.CopyToAsync(fileStream);
			}
		}
	}
}
