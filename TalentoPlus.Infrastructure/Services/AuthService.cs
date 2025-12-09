using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Infrastructure.Identity;

namespace TalentoPlus.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtService;

    public AuthService(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IEmailService emailService, IJwtTokenService jwtService)
    {
        _departmentRepository = departmentRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _jwtService = jwtService;
    }

    public async Task<string> RegisterEmployeeAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null) throw new InvalidOperationException("Departamento no válido");

        var employee = new Employee
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

        var user = new ApplicationUser
        {
            UserName = request.Document,
            Email = request.Email,
            Document = request.Document,
            EmailConfirmed = true,
            EmployeeId = employee.Id
        };

        // Create Identity user linked to the employee; Identity handles password hashing/validation.
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendAsync(request.Email, "Bienvenido a TalentoPlus", "Tu registro fue exitoso. Puedes iniciar sesión en la plataforma cuando sea habilitado.", cancellationToken);
        return await LoginAsync(new LoginRequest { Document = request.Document, Password = request.Password }, cancellationToken);
    }

    public async Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == request.Document, cancellationToken);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Minimal JWT payload to identify the employee and document on secured endpoints.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new("document", user.Document),
            new("employeeId", user.EmployeeId?.ToString() ?? string.Empty)
        };

        var token = _jwtService.GenerateToken(claims, DateTime.UtcNow.AddHours(8));
        return token;
    }
}
