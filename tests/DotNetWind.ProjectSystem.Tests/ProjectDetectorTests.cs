namespace DotNetWind.ProjectSystem.Tests;

public sealed class ProjectDetectorTests
{
    private const string ProjectDir = "/projects/MyApp";

    private static FakeFileSystem BuildFileSystem(string csprojContent, Action<FakeFileSystem>? configure = null)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(Path.Combine(ProjectDir, "MyApp.csproj"), csprojContent);
        configure?.Invoke(fs);
        return fs;
    }

    private static ProjectDetector CreateDetector(FakeFileSystem fs) =>
        new(fs, NullLogger<ProjectDetector>.Instance);

    [Fact]
    public async Task DetectAsync_WithBlazorWasmSdk_ReturnsBlazorWebAssembly()
    {
        var fs = BuildFileSystem(
            """<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>""",
            f =>
            {
                f.AddFile(Path.Combine(ProjectDir, "wwwroot", "index.html"), "<html/>");
                f.AddDirectory(ProjectDir);
            });

        var detector = CreateDetector(fs);
        var result = await detector.DetectAsync(Path.Combine(ProjectDir, "MyApp.csproj"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.ProjectType.ShouldBe(DotNetProjectType.BlazorWebAssembly);
        result.Value.TargetFramework.ShouldBe("net10.0");
    }

    [Fact]
    public async Task DetectAsync_WithBlazorWebAppStructure_ReturnsBlazorWebApp()
    {
        var fs = BuildFileSystem(
            """<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>""",
            f =>
            {
                f.AddFile(Path.Combine(ProjectDir, "Components", "App.razor"), "@code {}");
                f.AddDirectory(ProjectDir);
            });

        var detector = CreateDetector(fs);
        var result = await detector.DetectAsync(Path.Combine(ProjectDir, "MyApp.csproj"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.ProjectType.ShouldBe(DotNetProjectType.BlazorWebApp);
    }

    [Fact]
    public async Task DetectAsync_WithMvcStructure_ReturnsMvc()
    {
        var fs = BuildFileSystem(
            """<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>""",
            f =>
            {
                f.AddDirectory(Path.Combine(ProjectDir, "Controllers"));
                f.AddDirectory(Path.Combine(ProjectDir, "Views"));
                f.AddDirectory(ProjectDir);
            });

        var detector = CreateDetector(fs);
        var result = await detector.DetectAsync(Path.Combine(ProjectDir, "MyApp.csproj"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.ProjectType.ShouldBe(DotNetProjectType.Mvc);
    }

    [Fact]
    public async Task DetectAsync_WithRazorPagesStructure_ReturnsRazorPages()
    {
        var fs = BuildFileSystem(
            """<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>""",
            f =>
            {
                f.AddDirectory(Path.Combine(ProjectDir, "Pages"));
                f.AddFile(Path.Combine(ProjectDir, "Pages", "Shared", "_Layout.cshtml"), "<html/>");
                f.AddDirectory(ProjectDir);
            });

        var detector = CreateDetector(fs);
        var result = await detector.DetectAsync(Path.Combine(ProjectDir, "MyApp.csproj"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.ProjectType.ShouldBe(DotNetProjectType.RazorPages);
    }

    [Fact]
    public async Task DetectAsync_InvalidPath_ReturnsFailure()
    {
        var fs = new FakeFileSystem();
        var detector = CreateDetector(fs);
        var result = await detector.DetectAsync("/nonexistent/project.csproj");

        result.IsFailure.ShouldBeTrue();
    }
}
