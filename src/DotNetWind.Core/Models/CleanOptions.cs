namespace DotNetWind.Core.Models;

public sealed record CleanOptions(
    string? ProjectPath = null,
    string OutputCssRelativePath = "wwwroot/css/style.css",
    bool Verbose = false);
