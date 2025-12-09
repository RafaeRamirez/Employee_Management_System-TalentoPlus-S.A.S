using Microsoft.EntityFrameworkCore;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Infrastructure.Data;
using TalentoPlus.Infrastructure.Repositories;
using TalentoPlus.Infrastructure.Services;
using Xunit;

namespace TalentoPlus.Tests;

public class EmployeeServiceIntegrationTests
{
    private class FakeAiProvider : IAiProvider
    {
        public Task<string> BuildSqlLikeQueryAsync(string question, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("contar todos los empleados");
        }
    }

    [Fact]
    public async Task CreateEmployee_PersistsAndReturnsDto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        context.Departments.Add(new Domain.Entities.Department { Id = Guid.NewGuid(), Name = "Tecnología" });
        await context.SaveChangesAsync();

        var employeeRepo = new EmployeeRepository(context);
        var departmentRepo = new DepartmentRepository(context);
        var uow = new UnitOfWork(context);
        var service = new EmployeeService(employeeRepo, departmentRepo, uow, new FakeAiProvider());
        var employee = await service.CreateAsync(new EmployeeCreateRequest
        {
            Document = "123",
            FirstName = "Laura",
            LastName = "Mendez",
            Email = "laura@test.com",
            DepartmentId = context.Departments.First().Id,
            Position = "Developer",
            Salary = 5000,
            HireDate = DateTime.UtcNow.Date,
            Status = EmployeeStatus.Active
        });

        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal("Laura", employee.FirstName);
    }

    [Fact]
    public async Task DashboardMetrics_ReturnsCounts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var dept = new Domain.Entities.Department { Id = Guid.NewGuid(), Name = "Talento Humano" };
        context.Departments.Add(dept);
        context.Employees.Add(new Domain.Entities.Employee { Id = Guid.NewGuid(), Document = "1", FirstName = "A", LastName = "B", Email = "a@test.com", DepartmentId = dept.Id, Status = EmployeeStatus.Active, HireDate = DateTime.UtcNow, Position = "Analista" });
        context.Employees.Add(new Domain.Entities.Employee { Id = Guid.NewGuid(), Document = "2", FirstName = "C", LastName = "D", Email = "c@test.com", DepartmentId = dept.Id, Status = EmployeeStatus.Vacation, HireDate = DateTime.UtcNow, Position = "Analista" });
        await context.SaveChangesAsync();

        var employeeRepo = new EmployeeRepository(context);
        var departmentRepo = new DepartmentRepository(context);
        var uow = new UnitOfWork(context);
        var service = new EmployeeService(employeeRepo, departmentRepo, uow, new FakeAiProvider());
        var metrics = await service.GetDashboardMetricsAsync();

        Assert.Equal(2, metrics["total"]);
        Assert.Equal(1, metrics["vacation"]);
    }
}
