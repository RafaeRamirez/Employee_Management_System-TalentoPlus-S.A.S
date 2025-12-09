using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Contracts.Responses;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiProvider _aiProvider;

    public EmployeeService(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork, IAiProvider aiProvider)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _aiProvider = aiProvider;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        return employee is null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var hireDate = DateTime.SpecifyKind(request.HireDate, DateTimeKind.Utc);
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
            HireDate = hireDate,
            Status = request.Status,
            EducationLevel = request.EducationLevel,
            Profile = request.Profile,
            DepartmentId = request.DepartmentId
        };

        await _employeeRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var created = await _employeeRepository.GetByIdAsync(entity.Id, cancellationToken);
        return MapToDto(created!);
    }

    public async Task<EmployeeDto> UpdateAsync(EmployeeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null) throw new InvalidOperationException("Empleado no encontrado");

        entity.Document = request.Document;
        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Address = request.Address;
        entity.Position = request.Position;
        entity.Salary = request.Salary;
        entity.HireDate = DateTime.SpecifyKind(request.HireDate, DateTimeKind.Utc);
        entity.Status = request.Status;
        entity.EducationLevel = request.EducationLevel;
        entity.Profile = request.Profile;
        entity.DepartmentId = request.DepartmentId;

        await _employeeRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var updated = await _employeeRepository.GetByIdAsync(entity.Id, cancellationToken);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return;

        await _employeeRepository.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ImportFromExcelAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Validación básica: al menos 13 columnas con datos para mantener la estructura Documento..Departamento.
        var columnCount = worksheet.Row(1).CellsUsed().Count();
        if (columnCount < 14)
        {
            throw new InvalidOperationException("El archivo no tiene el número de columnas esperado (mínimo 14).");
        }

        // Each row becomes an upsert: create department if missing, update existing employee by Document or insert if new.
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var document = row.Cell("A").GetString().Trim();
            if (string.IsNullOrWhiteSpace(document)) continue;

            // New layout:
            // A Documento, B Nombres, C Apellidos, D FechaNacimiento (ignorado), E Direccion,
            // F Telefono, G Email, H Cargo, I Salario, J FechaIngreso, K Estado,
            // L NivelEducativo, M PerfilProfesional, N Departamento.
            var departmentName = row.Cell("N").GetString().Trim();
            var department = await _departmentRepository.GetByNameAsync(departmentName, cancellationToken);
            if (department == null)
            {
                department = new Department { Id = Guid.NewGuid(), Name = departmentName };
                await _departmentRepository.AddAsync(department, cancellationToken);
            }

            var employee = await _employeeRepository.GetByDocumentAsync(document, cancellationToken);
            var isNew = employee is null;
            employee ??= new Employee { Id = Guid.NewGuid(), Document = document };

            employee.FirstName = row.Cell("B").GetString();
            employee.LastName = row.Cell("C").GetString();
            // FechaNacimiento en columna D (no se almacena en el modelo actual, se ignora).
            employee.Address = row.Cell("E").GetString();
            employee.Phone = row.Cell("F").GetString();
            employee.Email = row.Cell("G").GetString();
            employee.Position = row.Cell("H").GetString();
            employee.Salary = decimal.TryParse(row.Cell("I").GetString(), CultureInfo.InvariantCulture, out var salary) ? salary : 0;
            var parsedHire = DateTime.TryParse(row.Cell("J").GetString(), out var hire) ? hire : DateTime.UtcNow.Date;
            employee.HireDate = DateTime.SpecifyKind(parsedHire, DateTimeKind.Utc);
            employee.Status = Enum.TryParse<EmployeeStatus>(row.Cell("K").GetString(), true, out var status) ? status : EmployeeStatus.Active;
            employee.EducationLevel = Enum.TryParse<EducationLevel>(row.Cell("L").GetString(), true, out var edu) ? edu : EducationLevel.None;
            employee.Profile = row.Cell("M").GetString();
            employee.DepartmentId = department.Id;

            if (isNew)
            {
                await _employeeRepository.AddAsync(employee, cancellationToken);
            }
            else
            {
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
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
        var total = await _employeeRepository.CountAsync(cancellationToken);
        var vacation = await _employeeRepository.CountByStatusAsync(EmployeeStatus.Vacation, cancellationToken);
        var active = await _employeeRepository.CountByStatusAsync(EmployeeStatus.Active, cancellationToken);

        return new Dictionary<string, int>
        {
            ["total"] = total,
            ["vacation"] = vacation,
            ["active"] = active
        };
    }

    public async Task<AiQueryResponse> AskAiAsync(string question, CancellationToken cancellationToken = default)
    {
        // External AI helps to interpret natural language; we always compute the final result with real DB queries.
        var query = await _aiProvider.BuildSqlLikeQueryAsync(question, cancellationToken);
        var normalized = query.ToLowerInvariant();
        var questionLower = question.ToLowerInvariant();

        var result = 0;
        // Cargo tiene prioridad cuando la pregunta menciona explícitamente el rol (ej: auxiliar).
        if (normalized.Contains("cargo") || questionLower.Contains("auxiliar"))
        {
            var role = ExtractQuotedValue(normalized);
            if (!string.IsNullOrWhiteSpace(role))
            {
                result = await _employeeRepository.CountByPositionAsync(role, cancellationToken);
            }
            else if (questionLower.Contains("auxiliar"))
            {
                result = await _employeeRepository.CountByPositionAsync("auxiliar", cancellationToken);
            }
        }
        else if (normalized.Contains("departamento"))
        {
            var departmentName = ExtractQuotedValue(normalized);
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                departmentName = ExtractDepartmentFromQuestion(questionLower);
            }
            if (!string.IsNullOrWhiteSpace(departmentName))
            {
                result = await _employeeRepository.CountByDepartmentNameAsync(departmentName, cancellationToken);
            }
        }
        else
        {
            if (normalized.Contains("estado"))
            {
                if (normalized.Contains("inactivo"))
                {
                    result = await _employeeRepository.CountByStatusAsync(EmployeeStatus.Inactive, cancellationToken);
                }
                else if (normalized.Contains("vacaciones"))
                {
                    result = await _employeeRepository.CountByStatusAsync(EmployeeStatus.Vacation, cancellationToken);
                }
                else
                {
                    result = await _employeeRepository.CountByStatusAsync(EmployeeStatus.Active, cancellationToken);
                }
            }
            else
            {
                result = await _employeeRepository.CountAsync(cancellationToken);
            }
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

    private static string ExtractDepartmentFromQuestion(string question)
    {
        // Busca "departamento de X" o palabras clave comunes.
        var idx = question.IndexOf("departamento de");
        if (idx >= 0)
        {
            var tail = question[(idx + "departamento de".Length)..].Trim();
            var parts = tail.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                // toma la primera palabra o combina dos si parece compuesto
                var candidate = parts[0];
                if (parts.Length > 1 && parts[1].Length > 3)
                    candidate = $"{parts[0]} {parts[1]}";
                return candidate.Trim('.', '?', '!', ',', ';');
            }
        }

        if (question.Contains("tecnolog"))
            return "tecnología";
        if (question.Contains("recursos humanos"))
            return "recursos humanos";
        if (question.Contains("operacion") || question.Contains("operación"))
            return "operaciones";

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
