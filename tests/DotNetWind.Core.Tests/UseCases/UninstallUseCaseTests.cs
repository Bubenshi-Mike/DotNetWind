using DotNetWind.Core.Abstractions;
using DotNetWind.Core.UseCases;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DotNetWind.Core.Tests.UseCases;

public sealed class UninstallUseCaseTests
{
    private const string ProjectDir = "/projects/MyApp";
    private const string ProjectPath = "/projects/MyApp/MyApp.csproj";
    private const string PackageJsonPath = "/projects/MyApp/package.json";

    [Fact]
    public async Task ExecuteAsync_WhenBackupEnabled_CreatesProjectAndPackageBackups()
    {
        var fileSystem = CreateFileSystem(projectExists: true, packageExists: true);
        var useCase = CreateUseCase(fileSystem);

        var result = await useCase.ExecuteAsync(new UninstallOptions(ProjectPath: ProjectPath));

        result.IsSuccess.ShouldBeTrue();
        fileSystem.Received().CopyFile(ProjectPath, ProjectPath + ".dotnetwind.bak", overwrite: true);
        fileSystem.Received().CopyFile(
            Arg.Is<string>(p => Normalize(p) == PackageJsonPath),
            Arg.Is<string>(p => Normalize(p) == PackageJsonPath + ".dotnetwind.bak"),
            overwrite: true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBackupDisabled_DoesNotCreateBackups()
    {
        var fileSystem = CreateFileSystem(projectExists: true, packageExists: true);
        var useCase = CreateUseCase(fileSystem);

        var result = await useCase.ExecuteAsync(new UninstallOptions(ProjectPath: ProjectPath, Backup: false));

        result.IsSuccess.ShouldBeTrue();
        fileSystem.DidNotReceiveWithAnyArgs().CopyFile(default!, default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDryRun_DoesNotCreateBackupsOrRemoveEntries()
    {
        var fileSystem = CreateFileSystem(projectExists: true, packageExists: true);
        var packageJsonManager = Substitute.For<IPackageJsonManager>();
        var useCase = CreateUseCase(fileSystem, packageJsonManager);

        var result = await useCase.ExecuteAsync(new UninstallOptions(ProjectPath: ProjectPath, DryRun: true));

        result.IsSuccess.ShouldBeTrue();
        fileSystem.DidNotReceiveWithAnyArgs().CopyFile(default!, default!, default);
        await packageJsonManager.DidNotReceiveWithAnyArgs().RemoveTailwindEntriesAsync(default!, default);
    }

    private static UninstallUseCase CreateUseCase(
        IFileSystem fileSystem,
        IPackageJsonManager? packageJsonManager = null)
    {
        var projectDetector = Substitute.For<IProjectDetector>();
        var projectFileUpdater = Substitute.For<IProjectFileUpdater>();
        packageJsonManager ??= Substitute.For<IPackageJsonManager>();

        projectDetector.DetectAsync(ProjectPath, Arg.Any<CancellationToken>())
            .Returns(Result<ProjectInfo>.Success(new ProjectInfo(
                ProjectPath,
                ProjectDir,
                "MyApp",
                "net10.0",
                DotNetProjectType.BlazorWebApp)));

        packageJsonManager.RemoveTailwindEntriesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        projectFileUpdater.RemoveTailwindBuildTargetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        return new UninstallUseCase(
            projectDetector,
            projectFileUpdater,
            packageJsonManager,
            fileSystem,
            NullLogger<UninstallUseCase>.Instance);
    }

    private static IFileSystem CreateFileSystem(bool projectExists, bool packageExists)
    {
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.FileExists(ProjectPath).Returns(projectExists);
        fileSystem.FileExists(PackageJsonPath).Returns(packageExists);
        fileSystem.FileExists(Arg.Is<string>(p => Normalize(p) == PackageJsonPath)).Returns(packageExists);
        return fileSystem;
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
