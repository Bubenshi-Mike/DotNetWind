using DotNetWind.Core.Models;
using Shouldly;

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
        var info = new ProjectInfo("/path/App.csproj", "/path", "App", "net9.0", type);
        info.GetDisplayName().ShouldBe(expected);
    }
}
