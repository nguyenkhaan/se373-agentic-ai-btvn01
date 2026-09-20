using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using IssueTriage.src.Configuration;
using IssueTriage.src.Contracts;
using IssueTriage.src.Helpers;
using Microsoft.VisualBasic;

namespace IssueTriage.src.Services;

public class FunctionCallingService(HttpClient httpClient , ModelConfiguration modelConfiguration)
{
    private static readonly Dictionary<string, string> ComponentOwner = new()
        {   
        ["payment"] = "checkout-platform",
        ["identity"] = "identity-platform",
        ["search"] = "search-platform",
    };
    private static readonly object[] FunctionTools = [
        new
            {
                type = "function",
                function = new
                {
                    name = "get_component_owner",
                    description = "Trả team chịu trách nhiệm cho một software component.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            component = new
                            {
                                type = "string",
                                @enum = new[] { "payment", "identity", "search" }
                            }
                        },
                        required = new[] { "component" },
                        additionalProperties = false
                    },
                    strict = true
                }
            }
        ];
    private const string SystemPrompt = """
        Bạn hỗ trợ triage issue phần mềm.
        Khi cần biết team xử lý một component, hãy gọi get_component_owner.
        Component hợp lệ là payment, identity hoặc search.
    """;
    public static bool TryGetComponentOwner(string component, out string owner)
    {
        return ComponentOwner.TryGetValue(component, out owner!);  //It's maybe null, so the ! make that our compiler confident on it never be null
    }
    private async Task<JsonDocument> CreateCompletionAsync(
        List<object> messages, bool requireTool, CancellationToken cancellationToken
    )
    {
             using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", modelConfiguration.ApiKey);

        request.Content = requireTool
            ? JsonContent.Create(new
            {
                model = modelConfiguration.Model,
                messages,
                tools = FunctionTools,
                tool_choice = "required",
            })
            : JsonContent.Create(new
            {
                model = modelConfiguration.Model,
                messages,
                tools = FunctionTools,
            });

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        return await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
    }
    private static string? GetNullableString(
        JsonElement element,
        string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
    public async Task<TriageResult> TriageIssue(
        string issue, CancellationToken cancellationToken
    )
    {
        var messages = new List<object>()
        {
            new { role = "system" , content = SystemPrompt },
            new { role = "user" , content = issue }
        };
        //The first calling 
        using var firstResponse = await CreateCompletionAsync(
            messages, requireTool : true, cancellationToken
        );
        var responseMessage = firstResponse.RootElement.GetProperty("choices")[0].GetProperty("message");
        if (!responseMessage.TryGetProperty("tool_calls", out var toolCalls) ||
            toolCalls.ValueKind != JsonValueKind.Array ||
            toolCalls.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Model returned no tool call despite tool_choice='required'.");
        }
        messages.Add(
            new { role = "assistant" , content = GetNullableString(responseMessage, "content") , tool_calls = toolCalls.Clone() }
        );
        var traces = new List<ToolTrace>();
        foreach (var toolCall in toolCalls.EnumerateArray())
        {
             var callId = toolCall.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Missing tool call id.");

            var function = toolCall.GetProperty("function");

            var name = function.GetProperty("name").GetString()
                ?? throw new InvalidOperationException("Missing function name.");

            var rawArguments = function.GetProperty("arguments").GetString()
                ?? throw new InvalidOperationException("Missing function arguments.");

            var arguments =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    rawArguments)
                ?? throw new InvalidOperationException(
                    "Tool arguments must be a JSON object.");

            var result = Tool.ExecuteToolCall(name, rawArguments);

            traces.Add(new ToolTrace(
                CallID: callId,
                Name: name,
                Arguments: arguments,
                Result: result));

            messages.Add(new
            {
                role = "tool",
                tool_call_id = callId,
                content = JsonSerializer.Serialize(result),
            });
        }

        //The second calling
        using var finalResponse = await CreateCompletionAsync(
            messages, requireTool : false, cancellationToken
        );
        var finalMessage = finalResponse.RootElement.GetProperty("choices")[0].GetProperty("message");
        var finalContent = GetNullableString(finalMessage, "content"); ;
        return new TriageResult(
            ToolTraces: traces,
            FinalResponse: finalContent
        );
    }
}