namespace OceanSurveyApi.Models
{
    public class ActivityEvent
    {
        public string? Type { get; set; }
        public double? X { get; set; }      // <- was int?
        public double? Y { get; set; }      // <- was int?
        public string? Target { get; set; }
        public string? Key { get; set; }
        public double? ScrollY { get; set; }
        public long? Time { get; set; }
    }
}
