using System.ComponentModel.DataAnnotations;

namespace AutoSats.Views.ViewModels
{
    public class NewScheduleViewModel
    {
        [Required]
        public string Key1 { get; set; } = string.Empty;

        [Required]
        public string Key2 { get; set; } = string.Empty;

        public string? Key3 { get; set; }
    }
}
