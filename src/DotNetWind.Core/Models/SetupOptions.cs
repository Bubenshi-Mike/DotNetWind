namespace DotNetWind.Core.Models;

public sealed record SetupOptions(
    string? ProjectPath = null,
    DotNetProjectType? ForcedProjectType = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool SkipNpmInstall = false,
    bool SkipNodeInstall = false,
    bool SkipBuild = false,
    bool Force = false,
    bool AssumeYes = false,
    bool DryRun = false,
    bool Verbose = false);
