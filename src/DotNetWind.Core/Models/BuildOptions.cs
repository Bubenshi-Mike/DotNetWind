namespace DotNetWind.Core.Models;

public sealed record BuildOptions(
    string? ProjectPath = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool Minify = false,
    bool Verbose = false);
