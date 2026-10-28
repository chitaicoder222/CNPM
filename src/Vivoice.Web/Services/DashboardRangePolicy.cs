// Purpose: Validates the time ranges accepted by administrative dashboard reporting.
// Responsibility: Prevents inverted date windows from reaching analytics queries or being represented as meaningful metrics.
namespace Vivoice.Web.Services;

public static class DashboardRangePolicy
{
    public static bool IsValid(DateTime fromUtc, DateTime toUtc) => fromUtc <= toUtc;
}
