namespace DotNetWind.ProjectSystem.Tests;

public sealed class ProjectFileUpdaterTests
{
    private const string ProjectDir = "/projects/MyApp";
    private const string CsprojPath = "/projects/MyApp/MyApp.csproj";

    private static string EmptyCsproj =>
        """
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
        </Project>
        """;

    private static ProjectFileUpdater CreateUpdater(FakeFileSystem fs) =>
        new(fs, NullLogger<ProjectFileUpdater>.Instance);

    private static ProjectInfo CreateProject(DotNetProjectType type = DotNetProjectType.BlazorWebApp) =>
        new(CsprojPath, ProjectDir, "MyApp", "net10.0", type);

    private static TailwindPaths CreatePaths() =>
        new(
            Path.Combine(ProjectDir, "Styles", "tailwind.css"),
            Path.Combine(ProjectDir, "wwwroot", "css", "style.css"),
            Path.Combine(ProjectDir, "package.json"),
            null);

    [Fact]
    public async Task AddTailwindBuildTargetAsync_WhenTargetMissing_AddsTarget()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(CsprojPath, EmptyCsproj);

        var updater = CreateUpdater(fs);
        var result = await updater.AddTailwindBuildTargetAsync(CreateProject(), CreatePaths());

        result.IsSuccess.ShouldBeTrue();
        var content = fs.GetWrittenContent(CsprojPath)!;
        content.ShouldContain($"Name=\"{MsBuildTargetTemplate.TargetName}\"");
    }

    [Fact]
    public async Task AddTailwindBuildTargetAsync_WhenTargetExists_IsIdempotent()
    {
        var existingContent = EmptyCsproj.Replace(
            "</Project>",
            $"""
              <Target Name="{MsBuildTargetTemplate.TargetName}" BeforeTargets="Build">
              </Target>
            </Project>
            """);

        var fs = new FakeFileSystem();
        fs.AddFile(CsprojPath, existingContent);

        var updater = CreateUpdater(fs);
        await updater.AddTailwindBuildTargetAsync(CreateProject(), CreatePaths());
        await updater.AddTailwindBuildTargetAsync(CreateProject(), CreatePaths());

        var content = fs.GetWrittenContent(CsprojPath) ?? existingContent;
        var occurrences = CountOccurrences(content, $"Name=\"{MsBuildTargetTemplate.TargetName}\"");
        occurrences.ShouldBe(1);
    }

    [Fact]
    public async Task HasTailwindBuildTargetAsync_WhenTargetPresent_ReturnsTrue()
    {
        var content = EmptyCsproj.Replace(
            "</Project>",
            $"<Target Name=\"{MsBuildTargetTemplate.TargetName}\" BeforeTargets=\"Build\"></Target></Project>");

        var fs = new FakeFileSystem();
        fs.AddFile(CsprojPath, content);

        var updater = CreateUpdater(fs);
        var hasTarget = await updater.HasTailwindBuildTargetAsync(CsprojPath);
        hasTarget.ShouldBeTrue();
    }

    [Fact]
    public async Task HasTailwindBuildTargetAsync_WhenTargetAbsent_ReturnsFalse()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(CsprojPath, EmptyCsproj);

        var updater = CreateUpdater(fs);
        var hasTarget = await updater.HasTailwindBuildTargetAsync(CsprojPath);
        hasTarget.ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveTailwindBuildTargetAsync_WhenTargetPresent_RemovesTarget()
    {
        var content = EmptyCsproj.Replace(
            "</Project>",
            $"<Target Name=\"{MsBuildTargetTemplate.TargetName}\" BeforeTargets=\"Build\"></Target></Project>");

        var fs = new FakeFileSystem();
        fs.AddFile(CsprojPath, content);

        var updater = CreateUpdater(fs);
        var result = await updater.RemoveTailwindBuildTargetAsync(CsprojPath);

        result.IsSuccess.ShouldBeTrue();
        fs.GetWrittenContent(CsprojPath)!.ShouldNotContain($"Name=\"{MsBuildTargetTemplate.TargetName}\"");
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
