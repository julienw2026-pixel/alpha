using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeetingScribe;

public sealed class SummaryClient
{
  private readonly HttpClient _http = new();

  public async Task<string> SummarizeAsync(string transcript)
  {
    var baseUrl = Environment.GetEnvironmentVariable("MEETING_SCRIBE_BASE_URL") ?? "";
    var apiKey = Environment.GetEnvironmentVariable("MEETING_SCRIBE_API_KEY") ?? "";
    var model = Environment.GetEnvironmentVariable("MEETING_SCRIBE_MODEL") ?? "claude-sonnet-4-5";

    if (string.IsNullOrWhiteSpace(baseUrl))
      return "Missing MEETING_SCRIBE_BASE_URL";
    if (string.IsNullOrWhiteSpace(apiKey))
      return "Missing MEETING_SCRIBE_API_KEY";

    var prompt =
      "You are a meeting assistant. Summarize the transcript in Chinese.\n" +
      "Return Markdown with sections: 摘要, 结论/决策, 待办(TODO, with owner and due if mentioned), 待澄清问题.\n\n" +
      transcript;

    // Anthropic Messages compatible payload
    var body = new
    {
      model,
      max_tokens = 1200,
      temperature = 0.2,
      messages = new[]
      {
        new { role = "user", content = prompt }
      }
    };

    var json = JsonSerializer.Serialize(body);

    // Try /v1/messages then /messages
    var url1 = baseUrl.TrimEnd('/') + "/v1/messages";
    var url2 = baseUrl.TrimEnd('/') + "/messages";

    var resText = await TryOnce(url1, apiKey, json);
    if (!resText.StartsWith("__HTTP_404__")) return resText;

    return await TryOnce(url2, apiKey, json);
  }

  private async Task<string> TryOnce(string url, string apiKey, string json)
  {
    using var req = new HttpRequestMessage(HttpMethod.Post, url);
    req.Headers.Add("x-api-key", apiKey);
    req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    req.Content = new StringContent(json, Encoding.UTF8, "application/json");

    using var resp = await _http.SendAsync(req);
    var text = await resp.Content.ReadAsStringAsync();

    if ((int)resp.StatusCode == 404)
      return "__HTTP_404__" + text;

    if (!resp.IsSuccessStatusCode)
      return $"HTTP {(int)resp.StatusCode}: {text}";

    // Anthropic response often has: { content: [ { type: 'text', text: '...' } ] }
    try
    {
      using var doc = JsonDocument.Parse(text);
      if (doc.RootElement.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
      {
        var sb = new StringBuilder();
        foreach (var part in content.EnumerateArray())
        {
          if (part.TryGetProperty("text", out var t)) sb.Append(t.GetString());
        }
        var s = sb.ToString().Trim();
        if (!string.IsNullOrEmpty(s)) return s;
      }
    }
    catch
    {
      // ignore
    }

    return text;
  }
}
