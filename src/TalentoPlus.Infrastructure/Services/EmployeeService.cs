using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Contracts.Responses;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IAiProvider _aiProvider;

    public EmployeeService(AppDbContext context, IAiProvider aiProvider)
    {
        _context = context;
        _aiProvider = aiProvider;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _context.Employees.Include(e => e.Department).ToListAsync(cancellationToken);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return employee is null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            Document = request.Document,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Position = request.Position,
            Salary = request.Salary,
            HireDate = request.HireDate,
            Status = request.Status,
            EducationLevel = request.EducationLevel,
            Profile = request.Profile,
            DepartmentId = request.DepartmentId
        };

        _context.Employees.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _context.Entry(entity).Reference(e => e.Department).LoadAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<EmployeeDto> UpdateAsync(EmployeeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (entity is null) throw new InvalidOperationException("Empleado no encontrado");

        entity.Document = request.Document;
        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Address = request.Address;
        entity.Position = request.Position;
        entity.Salary = request.Salary;
        entity.HireDate = request.HireDate;
        entity.Status = request.Status;
        entity.EducationLevel = request.EducationLevel;
        entity.Profile = request.Profile;
        entity.DepartmentId = request.DepartmentId;

        await _context.SaveChangesAsync(cancellationToken);
        await _context.Entry(entity).Reference(e => e.Department).LoadAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return;

        _context.Employees.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ImportFromExcelAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var headers = worksheet.Row(1).Cells().Select(c => c.GetString().Trim()).ToList();
        var expected = new[]
        {
            "Documento","Nombres","Apellidos","Correo","Telefono","Direccion","Cargo","Salario","FechaIngreso","Estado","NivelEducativo","Perfil","Departamento"
        };
        foreach (var col in expected)
        {
            if (!headers.Contains(col))
            {
                throw new InvalidOperationException($"Columna requerida no encontrada: {col}");
            }
        }

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var document = row.Cell("A").GetString().Trim();
            if (string.IsNullOrWhiteSpace(document)) continue;

            var departmentName = row.Cell("M").GetString().Trim();
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName, cancellationToken);
            if (department == null)
            {
                department = new Department { Id = Guid.NewGuid(), Name = departmentName };
                _context.Departments.Add(department);
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Document == document, cancellationToken);
            employee ??= new Employee { Id = Guid.NewGuid(), Document = document };

            employee.FirstName = row.Cell("B").GetString();
            employee.LastName = row.Cell("C").GetString();
            employee.Email = row.Cell("D").GetString();
            employee.Phone = row.Cell("E").GetString();
            employee.Address = row.Cell("F").GetString();
            employee.Position = row.Cell("G").GetString();
            employee.Salary = decimal.TryParse(row.Cell("H").GetString(), CultureInfo.InvariantCulture, out var salary) ? salary : 0;
            employee.HireDate = DateTime.TryParse(row.Cell("I").GetString(), out var hire) ? hire : DateTime.UtcNow.Date;
            employee.Status = Enum.TryParse<EmployeeStatus>(row.Cell("J").GetString(), true, out var status) ? status : EmployeeStatus.Active;
            employee.EducationLevel = Enum.TryParse<EducationLevel>(row.Cell("K").GetString(), true, out var edu) ? edu : EducationLevel.None;
            employee.Profile = row.Cell("L").GetString();
            employee.DepartmentId = department.Id;

            if (_context.Entry(employee).State == EntityState.Detached)
            {
                _context.Employees.Add(employee);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee is null) throw new InvalidOperationException("Empleado no encontrado");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("Hoja de Vida - TalentoPlus").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().Column(col =>
                {
                    col.Item().Text($"{employee.FirstName} {employee.LastName}").FontSize(18).SemiBold();
                    col.Item().Text($"Documento: {employee.Document}");
                    col.Item().Text($"Correo: {employee.Email} | Teléfono: {employee.Phone}");
                    col.Item().Text($"Dirección: {employee.Address}");
                    col.Item().Text($"Departamento: {employee.Department?.Name}");
                    col.Item().Text($"Cargo: {employee.Position} | Estado: {employee.Status}");
                    col.Item().Text($"Salario: {employee.Salary:C} | Ingreso: {employee.HireDate:yyyy-MM-dd}");
                    col.Item().Text($"Nivel educativo: {employee.EducationLevel}");
                    col.Item().Text($"Perfil: {employee.Profile}");
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<Dictionary<string, int>> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.Employees.CountAsync(cancellationToken);
        var vacation = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Vacation, cancellationToken);
        var active = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Active, cancellationToken);

        return new Dictionary<string, int>
        {
            ["total"] = total,
            ["vacation"] = vacation,
            ["active"] = active
        };
    }

    public async Task<AiQueryResponse> AskAiAsync(string question, CancellationToken cancellationToken = default)
    {
        var query = await _aiProvider.BuildSqlLikeQueryAsync(question, cancellationToken);
        var normalized = query.ToLowerInvariant();

        var result = 0;
        if (normalized.Contains("departamento"))
        {
            var departmentName = ExtractQuotedValue(normalized);
            if (!string.IsNullOrWhiteSpace(departmentName))
            {
                result = await _context.Employees.Include(e => e.Department)
                    .CountAsync(e => e.Department!.Name.ToLower() == departmentName.ToLower(), cancellationToken);
            }
        }
        else if (normalized.Contains("estado"))
        {
            if (normalized.Contains("inactivo"))
            {
                result = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Inactive, cancellationToken);
            }
            else if (normalized.Contains("vacaciones"))
            {
                result = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Vacation, cancellationToken);
            }
            else
            {
                result = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Active, cancellationToken);
            }
        }
        else if (normalized.Contains("cargo"))
        {
            var role = ExtractQuotedValue(normalized);
            if (!string.IsNullOrWhiteSpace(role))
            {
                result = await _context.Employees.CountAsync(e => e.Position.ToLower().Contains(role.ToLower()), cancellationToken);
            }
        }
        else
        {
            result = await _context.Employees.CountAsync(cancellationToken);
        }

        return new AiQueryResponse
        {
            Question = question,
            SqlLikeQuery = query,
            Result = result.ToString()
        };
    }

    private static string ExtractQuotedValue(string text)
    {
        var start = text.IndexOf('"');
        var end = text.LastIndexOf('"');
        if (start >= 0 && end > start)
        {
            return text[start..end].Trim('"');
        }

        return string.Empty;
    }

    private static EmployeeDto MapToDto(Employee employee) => new()
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
        DepartmentId = employee.DepartmentId,
        DepartmentName = employee.Department?.Name ?? string.Empty
    };
}
