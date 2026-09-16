using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Models;

public class JobApplication : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Company name must be between 2 and 100 characters.")]
    [Display(Name = "Company")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position title is required.")]
    [StringLength(
        150,
        MinimumLength = 2,
        ErrorMessage = "Position title must be between 2 and 150 characters.")]
    [Display(Name = "Position")]
    public string PositionTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Application source is required.")]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage = "Source must be between 2 and 50 characters.")]
    public string Source { get; set; } = string.Empty;

    [Url(ErrorMessage = "Enter a valid job URL.")]
    [StringLength(
        500,
        ErrorMessage = "Job URL cannot exceed 500 characters.")]
    [Display(Name = "Job URL")]
    public string? JobUrl { get; set; }

    [StringLength(
        100,
        ErrorMessage = "Location cannot exceed 100 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Applied date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Applied Date")]
    public DateTime AppliedDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(
        typeof(ApplicationStatus),
        ErrorMessage = "Select a valid application status.")]
    public ApplicationStatus Status { get; set; } =
        ApplicationStatus.Draft;

    [StringLength(
        1000,
        ErrorMessage = "Notes cannot exceed 1000 characters.")]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (AppliedDate.Date > DateTime.Today)
        {
            yield return new ValidationResult(
                "Applied date cannot be in the future.",
                new[] { nameof(AppliedDate) });
        }
    }
}
