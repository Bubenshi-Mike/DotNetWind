namespace DotNetWind.Core.UseCases;

public sealed class DoctorUseCase
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorUseCase> _logger;

    public DoctorUseCase(IDoctorService doctorService, ILogger<DoctorUseCase> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DoctorCheckResult>> ExecuteAsync(
        DoctorOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running doctor checks");
        return await _doctorService.RunChecksAsync(options, cancellationToken);
    }
}
