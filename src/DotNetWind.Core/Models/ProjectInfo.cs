namespace DotNetWind.Core.Models;

public sealed record ProjectInfo(
    string ProjectFilePath,
    string ProjectDirectory,
    string ProjectName,
    string? TargetFramework,
    DotNetProjectType ProjectType)
{
    public string GetDisplayName() => ProjectType switch
    {
        DotNetProjectType.BlazorWebAssembly => "Blazor WebAssembly",
        DotNetProjectType.BlazorServer => "Blazor Server",
        DotNetProjectType.BlazorWebApp => "Blazor Web App",
        DotNetProjectType.Mvc => "ASP.NET Core MVC",
        DotNetProjectType.RazorPages => "Razor Pages",
        DotNetProjectType.MauiHybrid => ".NET MAUI Hybrid",
        _ => "Unknown"
    };

    public string GetCssLink(string outputCssRelativePath = "wwwroot/css/style.css")
    {
        var href = ToWebPath(outputCssRelativePath);
        if (ProjectType is DotNetProjectType.Mvc or DotNetProjectType.RazorPages)
            href = "~/" + href;

        return $"""<link href="{href}" rel="stylesheet" />""";
    }

    public static string ToWebPath(string outputCssRelativePath)
    {
        var normalized = outputCssRelativePath.Replace('\\', '/').TrimStart('/');
        const string wwwrootPrefix = "wwwroot/";

        return normalized.StartsWith(wwwrootPrefix, StringComparison.OrdinalIgnoreCase)
            ? normalized[wwwrootPrefix.Length..]
            : normalized;
    }
}
