namespace DotNetWind.Core.Models;

public sealed record DoctorOptions(
    string? ProjectPath = null,
    string InputCssRelativePath = "Styles/tailwind.css",
    string OutputCssRelativePath = "wwwroot/css/style.css");
