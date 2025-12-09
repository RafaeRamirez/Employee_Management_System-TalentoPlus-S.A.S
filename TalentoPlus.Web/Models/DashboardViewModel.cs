using TalentoPlus.Application.Contracts.Responses;

namespace TalentoPlus.Web.Models;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int OnVacation { get; set; }
    public int ActiveEmployees { get; set; }
    public AiQueryResponse? AiResult { get; set; }
}
