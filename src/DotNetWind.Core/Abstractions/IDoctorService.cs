using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorCheckResult>> RunChecksAsync(
        string? projectPath,
        CancellationToken cancellationToken = default);
}
