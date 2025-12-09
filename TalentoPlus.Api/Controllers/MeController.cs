using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public MeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var employeeId = User.FindFirstValue("employeeId");
        if (string.IsNullOrWhiteSpace(employeeId) || !Guid.TryParse(employeeId, out var id))
        {
            return Unauthorized();
        }

        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> DownloadPdf(CancellationToken cancellationToken)
    {
        var employeeId = User.FindFirstValue("employeeId");
        if (string.IsNullOrWhiteSpace(employeeId) || !Guid.TryParse(employeeId, out var id))
        {
            return Unauthorized();
        }

        var pdf = await _employeeService.GeneratePdfAsync(id, cancellationToken);
        return File(pdf, "application/pdf", "HojaDeVida.pdf");
    }
}
