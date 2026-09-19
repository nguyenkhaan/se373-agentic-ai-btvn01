namespace IssueTriage.src.Helpers;

public class ComponentOwner
{
    private static readonly Dictionary<string, string> Owners = new()
    {
        ["payment"] = "checkout-platform",
        ["identity"] = "identity-platform",
        ["search"] = "search-platform",
    };
    public static bool TryGetComponentOwner(string component, out string owner)
    {
        return Owners.TryGetValue(component, out owner!);
    }
}
