namespace JobApplicationTracker.Services;

public record JwtTokenResult(
    string Token,
    DateTime ExpiresAtUtc);
    