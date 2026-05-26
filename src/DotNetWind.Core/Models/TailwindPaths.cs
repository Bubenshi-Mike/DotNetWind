namespace DotNetWind.Core.Models;

public sealed record TailwindPaths(
    string InputCssPath,
    string OutputCssPath,
    string PackageJsonPath,
    string? HostFilePath)
{
    public static TailwindPaths CreateDefault(string projectDirectory) => new(
        InputCssPath: Path.Combine(projectDirectory, "Styles", "tailwind.css"),
        OutputCssPath: Path.Combine(projectDirectory, "wwwroot", "css", "style.css"),
        PackageJsonPath: Path.Combine(projectDirectory, "package.json"),
        HostFilePath: null);
}
