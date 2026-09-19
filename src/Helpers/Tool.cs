using System.Text.Json;
namespace IssueTriage.src.Helpers;

public static class Tool
{
    public static Dictionary<string, string> ExecuteToolCall(string name, string rawArguments)
    {
        if (name != "get_component_owner")
            throw new Exception($"Tool is not allowed: {name}");
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawArguments);
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new ArgumentException("Tool arguments must be a JSON object");
        }
        using (document)
        {
            var root = document.RootElement; 
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Tool arguments must be a JSON object");
            }
            var properties = root.EnumerateObject().ToList();
            if (properties.Count != 1 || properties[0].Name != "component" || properties[0].Value.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Tool arguments must contain at least one string: component.");
            }
            var component = properties[0].Value.GetString();
            if (!ComponentOwner.TryGetComponentOwner(component, out var owner))
            {
                throw new ArgumentException($"Unknown component: {component}");
            }
            return new Dictionary<string, string>
            {
                ["component"] = component,
                ["owner"] = owner
            };
        };
    }
}
