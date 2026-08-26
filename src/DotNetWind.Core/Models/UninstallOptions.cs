namespace DotNetWind.Core.Models;

public sealed record UninstallOptions(
    string? ProjectPath = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool Force = false,
    bool Verbose = false);
