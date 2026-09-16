namespace JobApplicationTracker.Configuration;

public class ApplicationInfoOptions
{
    public const string SectionName = "ApplicationInfo";

    public string Name { get; set; } = string.Empty;
    public string Applicator { get; set; } = string.Empty;

    public int CopyrightYear { get; set; }
}
