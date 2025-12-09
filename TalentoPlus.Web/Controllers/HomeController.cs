using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Web.Models;

namespace TalentoPlus.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmployeeService _employeeService;

    public HomeController(ILogger<HomeController> logger, IEmployeeService employeeService)
    {
        _logger = logger;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Dashboard cards come from aggregated metrics in the service.
        var metrics = await _employeeService.GetDashboardMetricsAsync(cancellationToken);
        var viewModel = new DashboardViewModel
        {
            TotalEmployees = metrics.GetValueOrDefault("total"),
            OnVacation = metrics.GetValueOrDefault("vacation"),
            ActiveEmployees = metrics.GetValueOrDefault("active")
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AskAi(string question, CancellationToken cancellationToken)
    {
        var metrics = await _employeeService.GetDashboardMetricsAsync(cancellationToken);
        var response = await _employeeService.AskAiAsync(question, cancellationToken);

        var viewModel = new DashboardViewModel
        {
            TotalEmployees = metrics.GetValueOrDefault("total"),
            OnVacation = metrics.GetValueOrDefault("vacation"),
            ActiveEmployees = metrics.GetValueOrDefault("active"),
            AiResult = response
        };

        return View("Index", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
