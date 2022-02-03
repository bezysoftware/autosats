using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data;

[Owned]
public class ExchangeScheduleNotification
{
    [Required]
    public NotificationType Type { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string P256dh { get; set; } = string.Empty;

    [Required]
    public string Auth { get; set; } = string.Empty;
}
