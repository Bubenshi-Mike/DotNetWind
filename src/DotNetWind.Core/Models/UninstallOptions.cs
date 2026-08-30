namespace DotNetWind.Core.Models;

public sealed record UninstallOptions(
    string? ProjectPath = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool Force = false,
    bool DryRun = false,
    bool Backup = true,
    bool Verbose = false);
