using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        // Public endpoint: provides list of valid departments for registration forms.
        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }
}
