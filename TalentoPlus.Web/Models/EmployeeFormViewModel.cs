using System.ComponentModel.DataAnnotations;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Web.Models;

public class EmployeeFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public string Document { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow.Date;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public EducationLevel EducationLevel { get; set; } = EducationLevel.None;
    public string Profile { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}
