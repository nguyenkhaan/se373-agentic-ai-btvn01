using IssueTriage.src.Helpers;
namespace IssueTriage.src.Contracts;

public record TriageResult(IReadOnlyList<ToolTrace> ToolTraces , string? FinalResponse)
{ }
