using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Web.Models;
using System.Text;
using System.Globalization;

namespace TalentoPlus.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public EmployeesController(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return View(employees);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Departments = await _departmentService.GetAllAsync(cancellationToken);
        return View(new EmployeeFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        // Normaliza salario para admitir punto o coma.
        var salaryRaw = Request.Form["Salary"].ToString();
        if (!string.IsNullOrWhiteSpace(salaryRaw))
        {
            if (decimal.TryParse(salaryRaw, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed) ||
                decimal.TryParse(salaryRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                model.Salary = parsed;
                ModelState.Remove(nameof(model.Salary));
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync(cancellationToken);
            return View(model);
        }

        var request = new EmployeeCreateRequest
        {
            Document = model.Document,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            Position = model.Position,
            Salary = model.Salary,
            HireDate = model.HireDate,
            Status = model.Status,
            EducationLevel = model.EducationLevel,
            Profile = model.Profile,
            DepartmentId = model.DepartmentId
        };

        await _employeeService.CreateAsync(request, cancellationToken);
        TempData["SuccessMessage"] = $"Empleado {model.FirstName} {model.LastName} creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound();

        ViewBag.Departments = await _departmentService.GetAllAsync(cancellationToken);
        var model = new EmployeeFormViewModel
        {
            Id = employee.Id,
            Document = employee.Document,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            Status = employee.Status,
            EducationLevel = employee.EducationLevel,
            Profile = employee.Profile,
            DepartmentId = employee.DepartmentId
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        // Normaliza salario para admitir punto o coma.
        var salaryRaw = Request.Form["Salary"].ToString();
        if (!string.IsNullOrWhiteSpace(salaryRaw))
        {
            if (decimal.TryParse(salaryRaw, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed) ||
                decimal.TryParse(salaryRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                model.Salary = parsed;
                ModelState.Remove(nameof(model.Salary));
            }
        }

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync(cancellationToken);
            return View(model);
        }

        var request = new EmployeeUpdateRequest
        {
            Id = model.Id.Value,
            Document = model.Document,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            Position = model.Position,
            Salary = model.Salary,
            HireDate = model.HireDate,
            Status = model.Status,
            EducationLevel = model.EducationLevel,
            Profile = model.Profile,
            DepartmentId = model.DepartmentId
        };

        await _employeeService.UpdateAsync(request, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var pdf = await _employeeService.GeneratePdfAsync(id, cancellationToken);
        // Sólo descarga; se puede usar un nombre único para evitar caché y reflejar actualizaciones.
        var fileName = $"HojaDeVida-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Documento,Nombres,Apellidos,FechaNacimiento,Direccion,Telefono,Email,Cargo,Salario,FechaIngreso,Estado,NivelEducativo,PerfilProfesional,Departamento");

        foreach (var e in employees)
        {
            // CSV simple con comas; se escapan comillas dobles.
            string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

            sb.AppendLine(string.Join(",", new[]
            {
                Escape(e.Document),
                Escape(e.FirstName),
                Escape(e.LastName),
                string.Empty, // FechaNacimiento no almacenada
                Escape(e.Address),
                Escape(e.Phone),
                Escape(e.Email),
                Escape(e.Position),
                e.Salary.ToString(System.Globalization.CultureInfo.InvariantCulture),
                e.HireDate.ToString("yyyy-MM-dd"),
                Escape(e.Status.ToString()),
                Escape(e.EducationLevel.ToString()),
                Escape(e.Profile),
                Escape(e.DepartmentName)
            }));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"Empleados-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
