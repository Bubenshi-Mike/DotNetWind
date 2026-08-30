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
        content.ShouldContain("\"latest\"");
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
    public async Task RefreshManagedEntriesAsync_WhenFileExists_UpdatesDotNetWindEntries()
    {
        var existing =
            """
            {
              "name": "my-app",
              "scripts": {
                "start": "dotnet run",
                "tw:build": "tailwindcss -i old.css -o old.css",
                "tw:build:min": "tailwindcss -i old.css -o old.css --minify",
                "tw:watch": "tailwindcss -i old.css -o old.css --watch"
              },
              "devDependencies": {
                "tailwindcss": "3.4.0",
                "@tailwindcss/cli": "3.4.0",
                "vite": "latest"
              }
            }
            """;
        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, existing);
        var manager = CreateManager(fs);

        var result = await manager.RefreshManagedEntriesAsync(PackageJsonPath, "Styles/tailwind.css", "wwwroot/css/style.css");

        result.IsSuccess.ShouldBeTrue();
        var content = fs.GetWrittenContent(PackageJsonPath)!;
        content.ShouldContain("\"start\": \"dotnet run\"");
        content.ShouldContain("\"vite\": \"latest\"");
        content.ShouldContain("\"tw:build\": \"npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css\"");
        content.ShouldContain("\"tw:build:min\": \"npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --minify\"");
        content.ShouldContain("\"tw:watch\": \"npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --watch\"");
        content.ShouldContain("\"tailwindcss\": \"latest\"");
        content.ShouldContain("\"@tailwindcss/cli\": \"latest\"");
        content.ShouldNotContain("old.css");
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

    [Fact]
    public async Task RemoveTailwindEntriesAsync_RemovesOnlyDotNetWindEntries()
    {
        var json =
            """
            {
              "scripts": {
                "start": "dotnet run",
                "tw:build": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css",
                "tw:build:min": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --minify",
                "tw:watch": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --watch"
              },
              "devDependencies": {
                "tailwindcss": "latest",
                "@tailwindcss/cli": "latest",
                "vite": "latest"
              }
            }
            """;

        var fs = new FakeFileSystem();
        fs.AddFile(PackageJsonPath, json);
        var manager = CreateManager(fs);

        var result = await manager.RemoveTailwindEntriesAsync(PackageJsonPath);

        result.IsSuccess.ShouldBeTrue();
        var content = fs.GetWrittenContent(PackageJsonPath)!;
        content.ShouldContain("start");
        content.ShouldContain("vite");
        content.ShouldNotContain("tw:build");
        content.ShouldNotContain("tailwindcss");
        content.ShouldNotContain("@tailwindcss/cli");
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
