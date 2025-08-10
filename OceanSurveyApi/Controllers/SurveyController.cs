using Microsoft.AspNetCore.Mvc;
using OceanSurveyApi.Models;
using System.Text;
using System.Text.Json;
using System.IO.Compression;


namespace OceanSurveyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private static readonly object _lock = new();

        private readonly string _csvFilePath = "data.csv";
        private readonly string _activityLogPath = "activity.log";

        // Utility: Save activity events for a survey submission
        //private void LogActivity(List<ActivityEvent> events)
        //{
        //    if (events == null || events.Count == 0)
        //        return;

        //    lock (_lock)
        //    {
        //        using var sw = new StreamWriter(_activityLogPath, append: true, Encoding.UTF8);
        //        sw.WriteLine(JsonSerializer.Serialize(events));
        //    }
        //}


        // Utility: Save activity events for a survey submission with ID
        //private void LogActivity(int id, List<ActivityEvent> events)
        //{
        //    if (events == null || events.Count == 0)
        //        return;

        //    lock (_lock)
        //    {
        //        using var sw = new StreamWriter(_activityLogPath, append: true, Encoding.UTF8);
        //        // Write an object with ID and Events array per line
        //        var logEntry = new
        //        {
        //            ID = id,
        //            Events = events
        //        };
        //        sw.WriteLine(JsonSerializer.Serialize(logEntry));
        //    }
        //}

        [HttpGet("download/all")]
        public IActionResult DownloadAllFiles()
        {
            // List your files here
            var filesToInclude = new[] { "data.csv", "selected_images.csv", "activity_events.csv" };

            // Only include files that exist
            var files = filesToInclude.Where(f => System.IO.File.Exists(f)).ToList();
            if (files.Count == 0)
                return NotFound(new { success = false, message = "No survey files found." });

            // Create the ZIP archive in memory
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var filename in files)
                {
                    var entry = zip.CreateEntry(filename, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(filename);
                    fileStream.CopyTo(entryStream);
                }
            }
            ms.Position = 0; // reset stream to beginning

            // Download as 'survey_exports.zip'
            return File(ms.ToArray(), "application/zip", "survey_exports.zip");
        }


        private void LogActivity(int id, List<ActivityEvent> events)
        {
            if (events == null || events.Count == 0)
                return;

            string filePath = "activity_events.csv";
            bool fileExists = System.IO.File.Exists(filePath);

            lock (_lock)
            {
                using var sw = new StreamWriter(filePath, append: true, Encoding.UTF8);

                // Write the header once if file is new
                if (!fileExists)
                    sw.WriteLine("ID,Type,X,Y,Target,Key,ScrollY,Time");

                // Write one row per event
                foreach (var ev in events)
                {
                    // Escape possible commas/quotes for CSV
                    string target = ev.Target?.Replace("\"", "\"\"") ?? "";
                    string key = ev.Key?.Replace("\"", "\"\"") ?? "";

                    sw.WriteLine(
                        $"\"{id}\",\"{ev.Type}\",\"{ev.X}\",\"{ev.Y}\",\"{target}\",\"{key}\",\"{ev.ScrollY}\",\"{ev.Time}\""
                    );
                }
            }
        }




        [HttpPost]
        [Route("submit")]
        public IActionResult SubmitSurvey([FromBody] SurveySubmission submission)
        {
            if (submission == null)
                return BadRequest(new { success = false, message = "Submission cannot be null." });

            bool fileExists = System.IO.File.Exists(_csvFilePath);

            // CSV escaper
            static string Csv(string? s) => $"\"{(s ?? string.Empty).Replace("\"", "\"\"")}\"";

            lock (_lock)
            {
                using var sw = new StreamWriter(_csvFilePath, append: true, Encoding.UTF8);

                if (!fileExists)
                {
                    sw.WriteLine(string.Join(",",
                        "ID", "Name", "Age", "Profession", "Living Area", "Gender",
                        "TIPI1", "TIPI2", "TIPI3", "TIPI4", "TIPI5", "TIPI6", "TIPI7", "TIPI8", "TIPI9", "TIPI10",
                        "SelectedImages"
                    ));
                }

                var row = string.Join(",", new[]
                {
            Csv(submission.ID.ToString()),
            Csv(submission.Name),
            Csv(submission.Age.ToString()),
            Csv(submission.Profession),
            Csv(submission.LivingArea),
            Csv(submission.Gender),
            Csv(submission.TIPI1),
            Csv(submission.TIPI2),
            Csv(submission.TIPI3),
            Csv(submission.TIPI4),
            Csv(submission.TIPI5),
            Csv(submission.TIPI6),
            Csv(submission.TIPI7),
            Csv(submission.TIPI8),
            Csv(submission.TIPI9),
            Csv(submission.TIPI10),
            Csv(submission.SelectedImages)
        });

                sw.WriteLine(row);
            }

            // Save selected images in a separate CSV/log
            LogSelectedImages(submission.ID, submission.SelectedImages);

            // Save activity events (if any)
            if (submission.ActivityEvents != null && submission.ActivityEvents.Count > 0)
            {
                LogActivity(submission.ID, submission.ActivityEvents);
            }

            return Ok(new { success = true });
        }


        //[HttpPost]
        //[Route("submit")]
        //public IActionResult SubmitSurvey([FromBody] SurveySubmission submission)
        //{
        //    if (submission == null)
        //        return BadRequest(new { success = false, message = "Submission cannot be null." });

        //    bool fileExists = System.IO.File.Exists(_csvFilePath);

        //    lock (_lock)
        //    {
        //        using var sw = new StreamWriter(_csvFilePath, append: true, Encoding.UTF8);

        //        if (!fileExists)
        //        {
        //            sw.WriteLine("ID,Name,Age,Profession,Living Area,Gender,Openness,Conscientiousness,Extraversion,Agreeableness,Neuroticism,SelectedImages");
        //        }

        //        string row = $"\"{submission.ID}\",\"{submission.Name}\",\"{submission.Age}\",\"{submission.Profession}\",\"{submission.LivingArea}\",\"{submission.Gender}\",\"{submission.Openness}\",\"{submission.Conscientiousness}\",\"{submission.Extraversion}\",\"{submission.Agreeableness}\",\"{submission.Neuroticism}\",\"{(submission.SelectedImages ?? "").Replace("\"", "\"\"")}\"";
        //        sw.WriteLine(row);
        //    }

        //    // Save activity events for this survey, if any
        //    if (submission.ActivityEvents != null && submission.ActivityEvents.Count > 0)
        //    {
        //        LogActivity(submission.ID, submission.ActivityEvents);
        //    }

        //    // Save selected images in new CSV
        //    LogSelectedImages(submission.ID, submission.SelectedImages);

        //    // Save the activity events for this survey (if provided)
        //    if (submission.ActivityEvents != null && submission.ActivityEvents.Count > 0)
        //    {
        //        LogActivity(submission.ID, submission.ActivityEvents);
        //    }

        //    return Ok(new { success = true });
        //}



        private void LogSelectedImages(int id, string selectedImagesJson)
        {
            if (string.IsNullOrWhiteSpace(selectedImagesJson))
                return;

            // Parse the JSON array string into C# objects
            var imageSelections = JsonSerializer.Deserialize<List<SelectedImage>>(selectedImagesJson);
            if (imageSelections == null || imageSelections.Count == 0)
                return;

            string filePath = "selected_images.csv";
            bool fileExists = System.IO.File.Exists(filePath);

            lock (_lock)
            {
                using var sw = new StreamWriter(filePath, append: true, Encoding.UTF8);

                // Write the header once if file is new
                if (!fileExists)
                    sw.WriteLine("ID,ImageID,LikeStatus");

                // Write one row per image selection
                foreach (var img in imageSelections)
                {
                    sw.WriteLine($"\"{id}\",\"{img.id}\",\"{img.like}\"");
                }
            }
        }

        // Create this DTO to parse image selections
        private class SelectedImage
        {
            public string id { get; set; }
            public string like { get; set; }
        }




        //[HttpPost]
        //[Route("submit")]
        //public IActionResult SubmitSurvey([FromBody] SurveySubmission submission)
        //{
        //    if (submission == null)
        //        return BadRequest(new { success = false, message = "Submission cannot be null." });

        //    bool fileExists = System.IO.File.Exists(_csvFilePath);

        //    lock (_lock)
        //    {
        //        using var sw = new StreamWriter(_csvFilePath, append: true, Encoding.UTF8);

        //        if (!fileExists)
        //        {
        //            sw.WriteLine("ID,Name,Age,Profession,Living Area,Gender,Openness,Conscientiousness,Extraversion,Agreeableness,Neuroticism,SelectedImages");
        //        }

        //        string row = $"\"{submission.ID}\",\"{submission.Name}\",\"{submission.Age}\",\"{submission.Profession}\",\"{submission.LivingArea}\",\"{submission.Gender}\",\"{submission.Openness}\",\"{submission.Conscientiousness}\",\"{submission.Extraversion}\",\"{submission.Agreeableness}\",\"{submission.Neuroticism}\",\"{(submission.SelectedImages ?? "").Replace("\"", "\"\"")}\"";
        //        sw.WriteLine(row);
        //    }

        //    // Save the activity events for this survey (if provided)
        //    if (submission.ActivityEvents != null && submission.ActivityEvents.Count > 0)
        //    {
        //        LogActivity(submission.ActivityEvents);
        //    }

        //    return Ok(new { success = true });
        //}
    }
}
