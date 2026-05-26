namespace DotNetWind.Core.Models;

public sealed record DoctorCheckResult(
    string Name,
    DoctorStatus Status,
    string Message,
    string? Recommendation = null)
{
    public bool IsPass => Status == DoctorStatus.Pass;
    public bool IsWarning => Status == DoctorStatus.Warning;
    public bool IsFail => Status == DoctorStatus.Fail;
}
