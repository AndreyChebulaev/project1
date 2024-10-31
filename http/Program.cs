using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class SecurityUpdatesFetcher
{
    private static readonly HttpClient httpClient = new HttpClient();

    // Метод для получения обновлений по HTTP
    public async Task<string> GetUpdateViaHttpAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            return null;
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        SecurityUpdatesFetcher fetcher = new SecurityUpdatesFetcher();

        // Получение обновлений через HTTP
        string httpUrl = "http://alizar.habrahabr.ru/";
        string httpUpdate = await fetcher.GetUpdateViaHttpAsync(httpUrl);
        Console.WriteLine("HTTP Update:\n" + httpUpdate);
    }
}
