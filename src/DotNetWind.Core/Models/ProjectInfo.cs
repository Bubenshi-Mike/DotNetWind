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

    public string GetCssLink() =>
        ProjectType is DotNetProjectType.Mvc or DotNetProjectType.RazorPages
            ? """<link href="~/css/style.css" rel="stylesheet" />"""
            : """<link href="css/style.css" rel="stylesheet" />""";
}
