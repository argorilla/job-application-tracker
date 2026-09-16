using JobApplicationTracker.Models;

namespace JobApplicationTracker.ViewModels;

public class JobApplicationIndexViewModel
{
    public IReadOnlyList<JobApplication> Applications { get; set; } =
        [];

    public string? SearchTerm { get; set; }

    public ApplicationStatus? Status { get; set; }

    public int TotalCount { get; set; }

    public int PageSize { get; set; } = 10;

    public string? PreviousCursor { get; set; }

    public string? NextCursor { get; set; }

    public bool HasPreviousPage =>
        !string.IsNullOrWhiteSpace(PreviousCursor);

    public bool HasNextPage =>
        !string.IsNullOrWhiteSpace(NextCursor);
}
