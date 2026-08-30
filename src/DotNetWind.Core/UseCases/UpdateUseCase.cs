namespace DotNetWind.Core.UseCases;

public sealed class UpdateUseCase
{
    private readonly InitUseCase _initUseCase;
    private readonly ILogger<UpdateUseCase> _logger;

    public UpdateUseCase(InitUseCase initUseCase, ILogger<UpdateUseCase> logger)
    {
        _initUseCase = initUseCase;
        _logger = logger;
    }

    public async Task<Result<ProjectInfo>> ExecuteAsync(SetupOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting update use case");
        return await _initUseCase.ExecuteAsync(options with { RefreshPackageJsonEntries = true }, cancellationToken);
    }
}
