using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data;

public abstract class ExchangeEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public ExchangeSchedule Schedule { get; set; } = null!;

    [Required]
    public abstract ExchangeEventType Type { get; set; }

    public string? Error { get; set; }
}

public class ExchangeEventPause : ExchangeEvent
{
    public override ExchangeEventType Type { get; set; } = ExchangeEventType.Pause;
}

public class ExchangeEventResume : ExchangeEvent
{
    public override ExchangeEventType Type { get; set; } = ExchangeEventType.Resume;
}

public class ExchangeEventCreate : ExchangeEvent
{
    public override ExchangeEventType Type { get; set; } = ExchangeEventType.Create;
}
