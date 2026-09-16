using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Models;
public enum ApplicationStatus
{
    [Display(Name = "Draft")]
    Draft,

    [Display(Name = "Applied")]
    Applied,

    [Display(Name = "Interview")]
    Interview,

    [Display(Name = "Technical Test")]
    TechnicalTest,

    [Display(Name = "Offer")]
    Offer,

    [Display(Name = "Rejected")]
    Rejected,

    [Display(Name = "Withdrawn")]
    Withdrawn
}
