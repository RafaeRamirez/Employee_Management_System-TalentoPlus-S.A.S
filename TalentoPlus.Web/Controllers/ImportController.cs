using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Web.Controllers;

[Authorize]
public class ImportController : Controller
{
    private readonly IEmployeeService _employeeService;

    public ImportController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Selecciona un archivo válido.";
            return RedirectToAction(nameof(Index));
        }

        await _employeeService.ImportFromExcelAsync(file, cancellationToken);
        TempData["Message"] = "Importación exitosa.";
        return RedirectToAction(nameof(Index));
    }
}
