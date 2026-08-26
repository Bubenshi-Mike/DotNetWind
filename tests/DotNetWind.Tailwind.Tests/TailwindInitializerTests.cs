using DotNetWind.Core.Models;
using NSubstitute;

namespace DotNetWind.Tailwind.Tests;

public sealed class TailwindInitializerTests
{
    private const string ProjectDir = "/projects/MyApp";

    [Fact]
    public async Task InitializeAsync_WhenSkippingNpmAndBuild_DoesNotRequireNode()
    {
        var fileSystem = new FakeFileSystem();
        var packageJsonManager = Substitute.For<IPackageJsonManager>();
        var projectFileUpdater = Substitute.For<IProjectFileUpdater>();
        var processRunner = Substitute.For<IProcessRunner>();
        var tailwindRunner = Substitute.For<ITailwindRunner>();
        var nodeJsInstaller = Substitute.For<INodeJsInstaller>();

        packageJsonManager.CreateOrMergeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        projectFileUpdater.AddTailwindBuildTargetAsync(
                Arg.Any<ProjectInfo>(),
                Arg.Any<TailwindPaths>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var initializer = new TailwindInitializer(
            fileSystem,
            packageJsonManager,
            projectFileUpdater,
            processRunner,
            tailwindRunner,
            nodeJsInstaller,
            NullLogger<TailwindInitializer>.Instance);

        var project = new ProjectInfo(
            Path.Combine(ProjectDir, "MyApp.csproj"),
            ProjectDir,
            "MyApp",
            "net10.0",
            DotNetProjectType.BlazorWebApp);

        var paths = new TailwindPaths(
            Path.Combine(ProjectDir, "Styles", "tailwind.css"),
            Path.Combine(ProjectDir, "wwwroot", "css", "style.css"),
            Path.Combine(ProjectDir, "package.json"),
            null);

        var options = new SetupOptions(
            SkipNpmInstall: true,
            SkipBuild: true);

        var result = await initializer.InitializeAsync(project, paths, options);

        result.IsSuccess.ShouldBeTrue();
        await nodeJsInstaller.DidNotReceiveWithAnyArgs().EnsureNodeAndNpmAsync(
            default!,
            default,
            default,
            default);
    }
}
