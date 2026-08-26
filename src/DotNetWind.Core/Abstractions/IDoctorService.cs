namespace DotNetWind.Core.Abstractions;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorCheckResult>> RunChecksAsync(
        DoctorOptions options,
        CancellationToken cancellationToken = default);
}
