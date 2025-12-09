using TalentoPlus.Application.Contracts.Responses;
using TalentoPlus.Application.DTOs;

namespace TalentoPlus.Web.Models;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int OnVacation { get; set; }
    public int ActiveEmployees { get; set; }
    public AiQueryResponse? AiResult { get; set; }

    public IEnumerable<EmployeeDto> ActiveList { get; set; } = Enumerable.Empty<EmployeeDto>();
    public IEnumerable<EmployeeDto> InactiveList { get; set; } = Enumerable.Empty<EmployeeDto>();
    public IEnumerable<EmployeeDto> VacationList { get; set; } = Enumerable.Empty<EmployeeDto>();
}
