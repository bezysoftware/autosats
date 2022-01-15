using AntDesign.Charts;
using System.Text.Json.Serialization;

namespace AutoSats.Views.Shared.Schedules
{
    public class DualAxesConfigEx : DualAxesConfig
    {
        [Obsolete($"Use {nameof(Annotations)} instead.")]
        new public GuideLineConfig[]? GuideLine { get; set; }

        [JsonPropertyName("annotations")]
        public Dictionary<string, AnnotationConfig[]>? Annotations { get; set; }

        [JsonPropertyName("yAxis")]
        public object? YAxis { get; set; }
    }

    public class AnnotationConfig
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("start")]
        public object[]? Start { get; set; }
        
        [JsonPropertyName("end")]
        public object[]? End { get; set; }
        
        [JsonPropertyName("style")]
        public LineStyle? Style { get; set; }
        
        [JsonPropertyName("text")]
        public GuideLineConfigText? Text { get; set; }
    }
}
