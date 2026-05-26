namespace DotNetWind.Core.Models;

public sealed record WatchOptions(
    string? ProjectPath = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool Verbose = false);
