using DotNetWind.Core.Models;
using NSubstitute;

namespace DotNetWind.Tailwind.Tests;

public sealed class NodeJsInstallerTests
{
    [Fact]
    public async Task EnsureNodeAndNpmAsync_WhenAlreadyInstalled_DoesNotRunInstaller()
    {
        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync("node", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, "v22.0.0", ""));
        processRunner.RunAsync("npm", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, "10.0.0", ""));

        var installer = new NodeJsInstaller(processRunner, NullLogger<NodeJsInstaller>.Instance);

        var result = await installer.EnsureNodeAndNpmAsync("/project", allowInstall: false, skipInstall: false);

        result.IsSuccess.ShouldBeTrue();
        await processRunner.DidNotReceiveWithAnyArgs().RunStreamingAsync(
            default!,
            default!,
            default!,
            default,
            default,
            default);
    }

    [Fact]
    public async Task EnsureNodeAndNpmAsync_WhenMissingAndInstallNotAllowed_ReturnsUserCancelled()
    {
        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync("node", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));
        processRunner.RunAsync("npm", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));

        var installer = new NodeJsInstaller(processRunner, NullLogger<NodeJsInstaller>.Instance);

        var result = await installer.EnsureNodeAndNpmAsync("/project", allowInstall: false, skipInstall: false);

        result.IsFailure.ShouldBeTrue();
        result.ErrorKind.ShouldBe(ResultErrorKind.UserCancelled);
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain("--yes");
        await processRunner.DidNotReceiveWithAnyArgs().RunStreamingAsync(
            default!,
            default!,
            default!,
            default,
            default,
            default);
    }

    [Fact]
    public async Task EnsureNodeAndNpmAsync_WhenMissingAndSkipped_ReturnsMissingDependency()
    {
        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync("node", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));
        processRunner.RunAsync("npm", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));

        var installer = new NodeJsInstaller(processRunner, NullLogger<NodeJsInstaller>.Instance);

        var result = await installer.EnsureNodeAndNpmAsync("/project", allowInstall: true, skipInstall: true);

        result.IsFailure.ShouldBeTrue();
        result.ErrorKind.ShouldBe(ResultErrorKind.MissingDependency);
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain("Node.js LTS");
        await processRunner.DidNotReceiveWithAnyArgs().RunStreamingAsync(
            default!,
            default!,
            default!,
            default,
            default,
            default);
    }

    [Fact]
    public async Task EnsureNodeAndNpmAsync_WhenMissingAndInstallAllowed_ReturnsActionableFailureIfInstallCannotComplete()
    {
        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync("node", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));
        processRunner.RunAsync("npm", "--version", "/project", Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));
        processRunner.RunStreamingAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Action<string>?>(),
                Arg.Any<Action<string>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(127, "", "not found"));

        var installer = new NodeJsInstaller(processRunner, NullLogger<NodeJsInstaller>.Instance);

        var result = await installer.EnsureNodeAndNpmAsync("/project", allowInstall: true, skipInstall: false);

        result.IsFailure.ShouldBeTrue();
        result.ErrorKind.ShouldBe(ResultErrorKind.MissingDependency);
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain("Node.js LTS");
    }
}
