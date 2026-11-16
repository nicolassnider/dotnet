using DotNetMastery.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMastery.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiLifecycleController : ControllerBase
{
    private readonly ITransientOperation _transient1;
    private readonly ITransientOperation _transient2;
    private readonly IScopedOperation _scoped1;
    private readonly IScopedOperation _scoped2;
    private readonly ISingletonOperation _singleton1;
    private readonly ISingletonOperation _singleton2;

    public DiLifecycleController(
        ITransientOperation transient1,
        ITransientOperation transient2,
        IScopedOperation scoped1,
        IScopedOperation scoped2,
        ISingletonOperation singleton1,
        ISingletonOperation singleton2)
    {
        _transient1 = transient1;
        _transient2 = transient2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _singleton1 = singleton1;
        _singleton2 = singleton2;
    }

    [HttpGet]
    public IActionResult GetLifecycles()
    {
        return Ok(new
        {
            Transient = new
            {
                FirstInstanceId = _transient1.OperationId,
                SecondInstanceId = _transient2.OperationId,
                AreIdentical = _transient1.OperationId == _transient2.OperationId,
                Explanation = "Transient instances are ALWAYS recreated on each injection (different IDs)."
            },
            Scoped = new
            {
                FirstInstanceId = _scoped1.OperationId,
                SecondInstanceId = _scoped2.OperationId,
                AreIdentical = _scoped1.OperationId == _scoped2.OperationId,
                Explanation = "Scoped instances are shared WITHIN the same HTTP request (identical IDs)."
            },
            Singleton = new
            {
                FirstInstanceId = _singleton1.OperationId,
                SecondInstanceId = _singleton2.OperationId,
                AreIdentical = _singleton1.OperationId == _singleton2.OperationId,
                Explanation = "Singleton instances are shared across ALL requests application-wide (identical IDs)."
            }
        });
    }
}
