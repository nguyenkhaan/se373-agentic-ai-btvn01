using System.Text.Json;

namespace IssueTriage.src.Contracts;

public record ToolTrace(
    string CallID, string Name, Dictionary<string, JsonElement> Arguments, Dictionary<string, string> Result
)
{ }