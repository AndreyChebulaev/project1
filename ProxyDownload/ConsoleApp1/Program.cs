using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Введите URL для скачивания файла:");
        string url = Console.ReadLine();

        // Определение расширения файла из URL
        string fileExtension = Path.GetExtension(url);

        Console.WriteLine("Введите имя файла для сохранения:");
        string fileName0 = Console.ReadLine();
        string fileName1 = fileName0 + "rus" + fileExtension;
        string fileName2 = fileName0 + "eng" + fileExtension;
        // указать нужную папку для сохранения
        string directoryPath = "E:/T001/file";

        string filePath1 = Path.Combine(directoryPath, fileName1);
        string filePath2 = Path.Combine(directoryPath, fileName2);

        // Прокси для российского IP-адреса
        var russianProxy = new WebProxy("46.47.197.210:3128");

        // Прокси для иностранного IP-адреса
        var foreignProxy = new WebProxy("83.68.136.236:80");

        // Скачивание файла с российским IP
        await DownloadFileWithProxy(url, filePath1, russianProxy);

        // Скачивание файла с иностранным IP
        await DownloadFileWithProxy(url, filePath2, foreignProxy);
    }

    static async Task DownloadFileWithProxy(string url, string filePath, IWebProxy proxy)
    {
        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            //true для использования прокси
            UseProxy = true,
        };

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
