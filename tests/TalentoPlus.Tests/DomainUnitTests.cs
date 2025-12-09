using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using Xunit;

namespace TalentoPlus.Tests;

public class DomainUnitTests
{
    [Fact]
    public void Employee_DefaultStatus_IsActive()
    {
        var employee = new Employee();
        Assert.Equal(EmployeeStatus.Active, employee.Status);
    }

    [Fact]
    public void EducationLevel_EnumContainsExpectedValues()
    {
        Assert.Contains(nameof(EducationLevel.Masters), Enum.GetNames<EducationLevel>());
    }
}
