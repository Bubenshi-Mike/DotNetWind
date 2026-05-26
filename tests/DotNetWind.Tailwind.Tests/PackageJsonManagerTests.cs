using DotNetWind.Tailwind.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace DotNetWind.Tailwind.Tests;

public sealed class PackageJsonManagerTests
{
    private const string PackageJsonPath = "/projects/MyApp/package.json";

    private static PackageJsonManager CreateManager(FakeFileSystem fs) =>
        new(fs, NullLogger<PackageJsonManager>.Instance);

    [Fact]
    public async Task CreateOrMergeAsync_WhenFileDoesNotExist_CreatesNew()
    {
        var fs = new FakeFileSystem();
        var manager = CreateManager(fs);

        var result = await manager.CreateOrMergeAsync(PackageJsonPath, "Styles/tailwind.css", "wwwroot/css/style.css");

        result.IsSuccess.ShouldBeTrue();
        var content = fs.GetWrittenContent(PackageJsonPath);
        content.ShouldNotBeNull();
        content.ShouldContain("tw:build");
        content.ShouldContain("tailwindcss");
        content.ShouldContain("@tailwindcss/cli");
    }

    [Fact]
    public async Task CreateOrMergeAsync_WhenFileExists_MergesScripts()
    {
        var existing = """{"name": "my-app", "scripts": {"start": "dotnet run"}}""";
        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, existing);
        var manager = CreateManager(fs);

        var result = await manager.CreateOrMergeAsync(PackageJsonPath, "Styles/tailwind.css", "wwwroot/css/style.css");

        result.IsSuccess.ShouldBeTrue();
        var content = fs.GetWrittenContent(PackageJsonPath)!;
        content.ShouldContain("tw:build");
        content.ShouldContain("start");
    }

    [Fact]
    public async Task CreateOrMergeAsync_IsIdempotent()
    {
        var fs = new FakeFileSystem();
        var manager = CreateManager(fs);

        await manager.CreateOrMergeAsync(PackageJsonPath, "Styles/tailwind.css", "wwwroot/css/style.css");
        await manager.CreateOrMergeAsync(PackageJsonPath, "Styles/tailwind.css", "wwwroot/css/style.css");

        var content = fs.GetWrittenContent(PackageJsonPath)!;
        var twBuildCount = CountOccurrences(content, "tw:build:min");
        twBuildCount.ShouldBe(1);
    }

    [Fact]
    public async Task HasTailwindScriptsAsync_WhenScriptsPresent_ReturnsTrue()
    {
        var json = """{"scripts": {"tw:build": "npx @tailwindcss/cli -i input.css -o output.css"}}""";
        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, json);
        var manager = CreateManager(fs);

        var result = await manager.HasTailwindScriptsAsync(PackageJsonPath);
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasTailwindScriptsAsync_WhenScriptsAbsent_ReturnsFalse()
    {
        var json = """{"scripts": {"start": "dotnet run"}}""";
        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, json);
        var manager = CreateManager(fs);

        var result = await manager.HasTailwindScriptsAsync(PackageJsonPath);
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasTailwindDependenciesAsync_WhenDepsPresent_ReturnsTrue()
    {
        var json = """{"devDependencies": {"tailwindcss": "latest", "@tailwindcss/cli": "latest"}}""";
        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, json);
        var manager = CreateManager(fs);

        var result = await manager.HasTailwindDependenciesAsync(PackageJsonPath);
        result.ShouldBeTrue();
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
