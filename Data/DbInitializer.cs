using JobApplicationTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTracker.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context)
    {
        if (await context.JobApplications.AnyAsync())
        {
            return;
        }

        var applications = new List<JobApplication>
        {
            new()
            {
                CompanyName = "Northstar Labs",
                PositionTitle = "Full Stack Developer",
                Source = "Company Website",
                JobUrl = "https://example.com/jobs/full-stack-developer",
                Location = "Remote",
                AppliedDate = DateTime.Today.AddDays(-2),
                Status = ApplicationStatus.Applied,
                Notes = "Example application record for local development."
            },
            new()
            {
                CompanyName = "Northstar Labs",
                PositionTitle = "Full Stack Developer",
                Source = "Company Website",
                JobUrl = "https://example.com/jobs/mobile-developer",
                Location = "Remote",
                AppliedDate = DateTime.Today.AddDays(-2),
                Status = ApplicationStatus.Applied,
                Notes = "Example application record for local development."
            },
            new()
            {
                CompanyName = "Northstar Labs",
                PositionTitle = "Full Stack Developer",
                Source = "Company Website",
                JobUrl = "https://example.com/jobs/software-engineer",
                Location = "Remote",
                AppliedDate = DateTime.Today.AddDays(-2),
                Status = ApplicationStatus.Applied,
                Notes = "Example application record for local development."
            }
        };

        await context.JobApplications.AddRangeAsync(applications);
        await context.SaveChangesAsync();
    }
}
