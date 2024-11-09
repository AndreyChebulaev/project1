using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string httpUrl = "https://haveibeenpwned.com/api/v3/breaches";
        string ftpUrl = "ftp://ftp.ncbi.nlm.nih.gov/pub/README.ftp";
        string proxyUrl = "https://haveibeenpwned.com/api/v3/breaches"; // URL для загрузки через прокси
        string savePathHttp = "update_http.txt";
        string savePathFtp = "update_ftp.txt";
        string savePathProxyRus = "update_proxy_rus.txt";
        string savePathProxyEng = "update_proxy_eng.txt";
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

    // Метод для вычисления контрольной суммы
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
        var httpClientHandler = new HttpClientHandler { Proxy = proxy, UseProxy = true };

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
