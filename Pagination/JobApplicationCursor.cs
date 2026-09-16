namespace JobApplicationTracker.Pagination;

public sealed record JobApplicationCursor(
    DateTime AppliedDate,
    int Id);
    