using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Configuration;

public sealed class ThemeOptions
{
    public const string SectionName = "Theme";

    [EnumDataType(typeof(ThemeMode))]
    public ThemeMode DefaultMode { get; set; } = ThemeMode.System;

    public bool AllowUserSelection { get; set; } = true;
}
