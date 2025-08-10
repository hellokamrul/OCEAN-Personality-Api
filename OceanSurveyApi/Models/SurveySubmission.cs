namespace OceanSurveyApi.Models
{
    public class SurveySubmission
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Profession { get; set; }
        public string? LivingArea { get; set; }
        public string? Gender { get; set; }

        // TIPI raw responses (e.g., "Agree", "Strongly Disagree")
        public string? TIPI1 { get; set; } 
        public string? TIPI2 { get; set; }
        public string? TIPI3 { get; set; }
        public string? TIPI4 { get; set; }
        public string? TIPI5 { get; set; }
        public string? TIPI6 { get; set; }
        public string? TIPI7 { get; set; }
        public string? TIPI8 { get; set; } 
        public string? TIPI9 { get; set; }
        public string? TIPI10 { get; set; }

        //public string Openness { get; set; }
        //public string Conscientiousness { get; set; }
        //public string Extraversion { get; set; }
        //public string Agreeableness { get; set; }
        //public string Neuroticism { get; set; }
        public string? SelectedImages { get; set; } // JSON string (id/like array)
        public List<ActivityEvent>? ActivityEvents { get; set; } // List of activity events
    }
}
