using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentScanController : ControllerBase
{
    [HttpPost("scan")]
    public async Task<IActionResult> ScanDocument([FromForm] IFormFile file, [FromForm] string stepType)
    {
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            return Ok(new { success = false, error = "AI scanning not configured" });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var mimeType = file.ContentType;

            var extractionPrompt = GetExtractionPrompt(stepType);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image", source = new { type = "base64", media_type = mimeType, data = base64 } },
                            new { type = "text", text = extractionPrompt }
                        }
                    }
                }
            };

            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages",
                new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json"));

            var responseText = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(responseText);
            var contentArray = doc.RootElement.GetProperty("content");
            var content = contentArray.EnumerateArray().FirstOrDefault(e => e.GetProperty("type").GetString() == "text").GetProperty("text").GetString();

            var jsonMatch = Regex.Match(content ?? "", @"\{[\s\S]*\}");
            if (jsonMatch.Success)
            {
                return Ok(new { success = true, fields = JsonSerializer.Deserialize<object>(jsonMatch.Value), fileName = file.FileName, fileSize = file.Length });
            }
            return Ok(new { success = false, error = "Could not parse AI response", raw = content });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = $"Scan failed: {ex.Message}" });
        }
    }

    private static string GetExtractionPrompt(string stepType) => stepType switch
    {
        "driver_license" => "Extract these fields from this driver's license image. Return ONLY a JSON object with these keys: firstName, lastName, dateOfBirth, licenseNumber, licenseExpiry, licenseClass, endorsements, address. For firstName, use ONLY the first given name (ignore any middle names — e.g. if the name is 'STEVEN ALLAN' return only 'STEVEN'). For each field, provide an object with 'value' (string) and 'confidence' ('high', 'medium', or 'low'). If a field is not visible, set value to null.",
        "vehicle_registration" => "Extract these fields from this vehicle registration document. Return ONLY a JSON object with these keys: plateNumber, make, model, year, regoExpiry, vehicleType. For each field, provide an object with 'value' (string) and 'confidence' ('high', 'medium', or 'low'). If a field is not visible, set value to null.",
        "vehicle_insurance" => "Extract these fields from this vehicle insurance document. Return ONLY a JSON object with these keys: policyNumber, insurerName, coverageAmount, expiryDate. For each field, provide an object with 'value' (string) and 'confidence' ('high', 'medium', or 'low'). If a field is not visible, set value to null.",
        "wof_certificate" => "Extract these fields from this Warrant of Fitness (WOF) certificate/sticker. Return ONLY a JSON object with these keys: wofExpiry. For each field, provide an object with 'value' (string) and 'confidence' ('high', 'medium', or 'low'). If a field is not visible, set value to null.",
        "tsl_certificate" => "Extract these fields from this Transport Service License (TSL) document. Return ONLY a JSON object with these keys: tslNumber, tslExpiry. For each field, provide an object with 'value' (string) and 'confidence' ('high', 'medium', or 'low'). If a field is not visible, set value to null.",
        _ => "Extract any text and data from this document image. Return a JSON object with identified field names as keys, each containing 'value' and 'confidence'."
    };
}
