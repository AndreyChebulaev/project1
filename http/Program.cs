using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string httpUrl = "http://alizar.habrahabr.ru/"; 
        string ftpUrl = "ftp://ftp.ncbi.nlm.nih.gov/pub/README.ftp";    
        string savePathHttp = "update_http.txt";
        string savePathFtp = "update_ftp.txt";

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
}
