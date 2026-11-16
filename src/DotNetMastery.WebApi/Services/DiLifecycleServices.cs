namespace DotNetMastery.WebApi.Services;

/// <summary>
/// Services demonstrating the 3 core Dependency Injection lifecycles in .NET:
/// - Transient: Created each time they are requested from the container.
/// - Scoped: Created once per client request (HTTP connection lifecycle).
/// - Singleton: Created once on first request and shared application-wide across all requests.
/// </summary>
public interface ITransientOperation
{
    Guid OperationId { get; }
}

public interface IScopedOperation
{
    Guid OperationId { get; }
}

public interface ISingletonOperation
{
    Guid OperationId { get; }
}

public class OperationService : ITransientOperation, IScopedOperation, ISingletonOperation
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

public class DiLifecycleReportService
{
    public ITransientOperation TransientService { get; }
    public IScopedOperation ScopedService { get; }
    public ISingletonOperation SingletonService { get; }

    public DiLifecycleReportService(
        ITransientOperation transientService,
        IScopedOperation scopedService,
        ISingletonOperation singletonService)
    {
        TransientService = transientService;
        ScopedService = scopedService;
        SingletonService = singletonService;
    }
}
