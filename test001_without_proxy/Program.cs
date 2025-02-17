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
        string savePathHttp = "update_http.txt";
        string savePathFtp = "update_ftp.txt";
        string checksumsFilePath = "checksums.txt";

        // Загрузка через HTTP
        try
        {
            await DownloadFileHttpAsync(httpUrl, savePathHttp);
            Console.WriteLine($"Файл успешно загружен по HTTP и сохранен как {savePathHttp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки через HTTP: {ex.Message}");
        }

        // Загрузка через FTP
        try
        {
            DownloadFileFtp(ftpUrl, savePathFtp);
            Console.WriteLine($"Файл успешно загружен по FTP и сохранен как {savePathFtp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки через FTP: {ex.Message}");
        }

        // Подсчет и сохранение контрольных сумм
        try
        {
            using (StreamWriter writer = new StreamWriter(checksumsFilePath))
            {
                string checksumHttp = CalculateChecksum(savePathHttp);
                string checksumFtp = CalculateChecksum(savePathFtp);

                // Запись контрольных сумм
                writer.WriteLine($"Контрольные суммы обновлений безопасности:");
                writer.WriteLine($"{savePathHttp}: {checksumHttp}");
                writer.WriteLine($"{savePathFtp}: {checksumFtp}");
                Console.WriteLine($"Контрольные суммы сохранены в файле '{checksumsFilePath}'");

                // Сравнение и запись результатов
                if (checksumHttp == checksumFtp)
                {
                    writer.WriteLine("Обновления безопасности сайтов идентичны на основе контрольных сумм.");
                    Console.WriteLine("Обновления безопасности сайтов идентичны на основе контрольных сумм.");
                }
                else
                {
                    writer.WriteLine("Обновления безопасности сайтов различаются на основе контрольных сумм.");
                    Console.WriteLine("Обновления безопасности сайтов различаются на основе контрольных сумм.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при вычислении контрольной суммы: {ex.Message}");
        }
    }

    // Метод для загрузки текстового файла через HTTP
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

    // Метод для загрузки текстового файла через FTP
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

    // Метод для расчета контрольной суммы файла с использованием SHA-256
    static string CalculateChecksum(string filePath)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
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
    }
}
