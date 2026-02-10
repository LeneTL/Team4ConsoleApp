// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;
using System.Text.Json;

var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var filePath = "events.jsonl";

string currentStudentId = "";

using var reader = new StreamReader(filePath);
string? line;

Console.WriteLine("Starter import...");

while ((line = await reader.ReadLineAsync()) != null)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    var eventData = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
    if (eventData == null) continue;

    var type = eventData["type"].ToString();

    if (type == "student_registrert")
    {

        var response = await httpClient.PostAsJsonAsync("/events", eventData);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (result.TryGetProperty("studentId", out var id))
        {
            currentStudentId = id.GetString()!;
            Console.WriteLine($"Registrert ny student. Fikk ID: {currentStudentId}");
        }
    }
    else
    {
        eventData["studentId"] = currentStudentId;

        var response = await httpClient.PostAsJsonAsync("/events", eventData);

        if (response.IsSuccessStatusCode)
            Console.WriteLine($"Sendte hendelse: {type} for student {currentStudentId}");
    }
}

Console.WriteLine("Import ferdig!");
