using System.ComponentModel.DataAnnotations;

namespace AutoSats.Views.ViewModels
{
    public class NewSchedule
    {
        [Required]
        public ExchangeKeys? ExchangeKeys { get; set; }

        [Required] 
        public ScheduleDetails? ScheduleDetails { get; set; }
    }
}
