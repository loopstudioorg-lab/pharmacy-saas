using Microsoft.AspNetCore.Mvc;

namespace PMS.Api.Controllers;

/// <summary>
/// Phase 1 placeholder controllers. Each one returns 501 Not Implemented to confirm the
/// cloud surface exists; real implementations land in Phase 6 (blueprint Section 18).
/// </summary>
public abstract class PlaceholderControllerBase : ControllerBase
{
    protected IActionResult NotImplementedYet(string controllerName) =>
        StatusCode(501, new
        {
            error = "not_implemented",
            controller = controllerName,
            message = $"{controllerName} is a placeholder. Real implementation lands in Phase 6.",
        });
}

[ApiController]
[Route("api/auth")]
public class AuthController : PlaceholderControllerBase
{
    [HttpPost("login")]
    public IActionResult Login() => NotImplementedYet(nameof(AuthController));
}

[ApiController]
[Route("api/license")]
public class LicenseController : PlaceholderControllerBase
{
    [HttpPost("validate")]
    public IActionResult Validate() => NotImplementedYet(nameof(LicenseController));

    [HttpPost("renew")]
    public IActionResult Renew() => NotImplementedYet(nameof(LicenseController));
}

[ApiController]
[Route("api/machines")]
public class MachineController : PlaceholderControllerBase
{
    [HttpPost("register")]
    public IActionResult Register() => NotImplementedYet(nameof(MachineController));

    [HttpPost("replace")]
    public IActionResult Replace() => NotImplementedYet(nameof(MachineController));
}

[ApiController]
[Route("api/sync")]
public class SyncController : PlaceholderControllerBase
{
    [HttpPost("push")]
    public IActionResult Push() => NotImplementedYet(nameof(SyncController));

    [HttpGet("pull")]
    public IActionResult Pull() => NotImplementedYet(nameof(SyncController));
}

[ApiController]
[Route("api/backup")]
public class BackupController : PlaceholderControllerBase
{
    [HttpPost("upload")]
    public IActionResult Upload() => NotImplementedYet(nameof(BackupController));

    [HttpGet("download")]
    public IActionResult Download() => NotImplementedYet(nameof(BackupController));
}

[ApiController]
[Route("api/reports")]
public class ReportsController : PlaceholderControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult Dashboard() => NotImplementedYet(nameof(ReportsController));
}
