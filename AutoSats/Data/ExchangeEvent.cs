using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSats.Data
{
    public abstract class ExchangeEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public ExchangeSchedule Schedule { get; set; } = null!;

        public string? Error { get; set; }
    }
}