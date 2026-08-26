namespace DotNetWind.Core.Tests.Models;

public sealed class ProjectInfoTests
{
    [Theory]
    [InlineData(DotNetProjectType.BlazorWebAssembly, "Blazor WebAssembly")]
    [InlineData(DotNetProjectType.BlazorServer, "Blazor Server")]
    [InlineData(DotNetProjectType.BlazorWebApp, "Blazor Web App")]
    [InlineData(DotNetProjectType.Mvc, "ASP.NET Core MVC")]
    [InlineData(DotNetProjectType.RazorPages, "Razor Pages")]
    [InlineData(DotNetProjectType.MauiHybrid, ".NET MAUI Hybrid")]
    [InlineData(DotNetProjectType.Unknown, "Unknown")]
    public void GetDisplayName_ShouldReturnCorrectLabel(DotNetProjectType type, string expected)
    {
        var info = new ProjectInfo("/path/App.csproj", "/path", "App", "net10.0", type);
        info.GetDisplayName().ShouldBe(expected);
    }

    [Theory]
    [InlineData(DotNetProjectType.BlazorWebApp, "wwwroot/css/app.css", """<link href="css/app.css" rel="stylesheet" />""")]
    [InlineData(DotNetProjectType.BlazorWebAssembly, "wwwroot/assets/site.css", """<link href="assets/site.css" rel="stylesheet" />""")]
    [InlineData(DotNetProjectType.Mvc, "wwwroot/css/app.css", """<link href="~/css/app.css" rel="stylesheet" />""")]
    [InlineData(DotNetProjectType.RazorPages, "assets/site.css", """<link href="~/assets/site.css" rel="stylesheet" />""")]
    public void GetCssLink_ShouldUseConfiguredOutputPath(DotNetProjectType type, string outputPath, string expected)
    {
        var info = new ProjectInfo("/path/App.csproj", "/path", "App", "net10.0", type);
        info.GetCssLink(outputPath).ShouldBe(expected);
    }
}
