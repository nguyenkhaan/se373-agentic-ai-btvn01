using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IssueTriage.src.Configuration;
using IssueTriage.src.Contracts;

namespace IssueTriage.src.Services;

public class MinimalTriageService
{
    private readonly HttpClient _httpClient;
    private readonly ModelConfiguration _modelConfiguration;

    public MinimalTriageService(HttpClient httpClient, ModelConfiguration configuration)
    {
        _httpClient = httpClient;
        _modelConfiguration = configuration;
    }
    public async Task<MinimalTriageResponse> GetAnswer(string issue , CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _modelConfiguration.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _modelConfiguration.Model,
            messages = new[]
            {
                new { role = "system" , content = "Bạn là một người hỗ trợ phân loại lỗi phần mềm" },
                new { role = "user" , content = issue }
            }
        });
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
        var answer = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return new MinimalTriageResponse
        {
            Answer = answer
        };
    }
}
