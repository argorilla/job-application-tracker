using JobApplicationTracker.Models;

namespace JobApplicationTracker.ViewModels;

public class HomeDashboardViewModel
{
    public int TotalApplications { get; set; }

    public IReadOnlyDictionary<ApplicationStatus, int> StatusCounts
        { get; set; } = new Dictionary<ApplicationStatus, int>();

    public IReadOnlyList<JobApplication> RecentApplications
        { get; set; } = [];

    public int GetStatusCount(ApplicationStatus status)
    {
        return StatusCounts.GetValueOrDefault(status);
    }

    public int ActiveApplications =>
        GetStatusCount(ApplicationStatus.Applied) +
        GetStatusCount(ApplicationStatus.Interview) +
        GetStatusCount(ApplicationStatus.TechnicalTest);
}
