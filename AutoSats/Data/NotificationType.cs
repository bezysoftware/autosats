using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data;

public enum NotificationType
{
    [Display(Name = "Never")]
    None,

    [Display(Name = "Only on error")]
    Errors,

    [Display(Name = "On each event")]
    All
}
