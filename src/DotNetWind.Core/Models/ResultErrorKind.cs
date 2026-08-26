namespace DotNetWind.Core.Models;

public enum ResultErrorKind
{
    General = 0,
    Validation = 1,
    UnsupportedProjectType = 2,
    MissingDependency = 3,
    UserCancelled = 4
}
