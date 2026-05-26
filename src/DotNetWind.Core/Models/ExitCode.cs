namespace DotNetWind.Core.Models;

public static class ExitCode
{
    public const int Success = 0;
    public const int GeneralFailure = 1;
    public const int ValidationFailed = 2;
    public const int UnsupportedProjectType = 3;
    public const int MissingDependency = 4;
    public const int UserCancelled = 5;
}
