using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/security-updates")]
public class SecurityUpdateController : ControllerBase
{
	private readonly HttpClient _httpClient;

	public SecurityUpdateController(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	[HttpPost("download-and-compare")]
	public async Task<IActionResult> DownloadAndCompare([FromBody] UpdateRequest request)
	{
		if (request.Urls == null || request.Urls.Length < 2)
			return BadRequest("Необходимо предоставить минимум два URL для сравнения.");

		var checksums = new Dictionary<string, string>();

		foreach (var url in request.Urls)
		{
			try
			{
				var data = await _httpClient.GetByteArrayAsync(url);
				var checksum = ComputeSHA256(data);
				checksums[url] = checksum;
			}
			catch (Exception ex)
			{
				return BadRequest($"Ошибка при загрузке {url}: {ex.Message}");
			}
		}

		var allEqual = checksums.Values.Distinct().Count() == 1;
		return Ok(new { Checksums = checksums, Identical = allEqual });
	}

	private string ComputeSHA256(byte[] data)
	{
		using (var sha256 = SHA256.Create())
		{
			var hash = sha256.ComputeHash(data);
			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}
	}
}

public class UpdateRequest
{
	public string[] Urls { get; set; }
}
