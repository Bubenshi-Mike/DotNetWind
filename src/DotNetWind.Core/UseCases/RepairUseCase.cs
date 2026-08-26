namespace DotNetWind.Core.UseCases;

public sealed class RepairUseCase
{
    private readonly InitUseCase _initUseCase;
    private readonly ILogger<RepairUseCase> _logger;

    public RepairUseCase(InitUseCase initUseCase, ILogger<RepairUseCase> logger)
    {
        _initUseCase = initUseCase;
        _logger = logger;
    }

    public async Task<Result<ProjectInfo>> ExecuteAsync(SetupOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting repair use case");
        return await _initUseCase.ExecuteAsync(options, cancellationToken);
    }
}
