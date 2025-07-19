using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.IO;

var baseUrl = "http://localhost:8000/api/v1";
var username = "admin";
var password = "wexflow2018";
var workflowId = 41;

using var httpClient = new HttpClient();

async Task<string> LoginAsync(string user, string pass, bool stayConnected = false)
{
  var payload = new
  {
    username = user,
    password = pass,
    stayConnected = stayConnected
  };

  var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
  var response = await httpClient.PostAsync($"{baseUrl}/login", content);

  if (!response.IsSuccessStatusCode)
    throw new Exception($"Login failed: HTTP {response.StatusCode} - {response.ReasonPhrase}");

  var json = await response.Content.ReadAsStringAsync();
  var doc = JsonDocument.Parse(json);
  return doc.RootElement.GetProperty("access_token").GetString();
}

async Task<string> StartWorkflowAsync(string token, int workflowId)
{
  var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/start?w={workflowId}");
  request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

  var response = await httpClient.SendAsync(request);
  if (!response.IsSuccessStatusCode)
    throw new Exception($"Start failed: HTTP {response.StatusCode} - {response.ReasonPhrase}");

  var json = await response.Content.ReadAsStringAsync();
  return JsonSerializer.Deserialize<string>(json);
}

async Task ListenToSseAsync(string url, string token)
{
  var request = new HttpRequestMessage(HttpMethod.Get, url);
  request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
  request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

  var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
  using var stream = await response.Content.ReadAsStreamAsync();
  using var reader = new StreamReader(stream);

  Console.WriteLine("SSE connection opened");

  while (!reader.EndOfStream)
  {
    var line = await reader.ReadLineAsync();
    if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data: "))
    {
      var json = line.Substring("data: ".Length);
      try
      {
        var doc = JsonDocument.Parse(json);
        Console.WriteLine("Received SSE JSON:");
        Console.WriteLine(doc.RootElement.ToString());
        break; // Close after first message
      }
      catch (Exception ex)
      {
        Console.WriteLine("Failed to parse SSE JSON: " + ex.Message);
      }
    }
  }

  Console.WriteLine("SSE connection closed");
}

try
{
  var token = await LoginAsync(username, password);
  var jobId = await StartWorkflowAsync(token, workflowId);

  Console.WriteLine($"Workflow {workflowId} started. Job ID: {jobId}");

  var sseUrl = $"{baseUrl}/sse/{workflowId}/{jobId}";
  await ListenToSseAsync(sseUrl, token);
}
catch (Exception ex)
{
  Console.WriteLine("Error: " + ex.Message);
}
