using System.Globalization;
using System.Text;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation.Domain;

public static class AnimalSummary
{
    public static string Create(AnimalDocument document, DateTime generatedAt)
    {
        var animals = document.Records.Select(record => record.Animal).ToArray();
        var report = new StringBuilder();
        report.AppendLine("BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE");
        report.AppendLine("ANIMAL SUMMARY REPORT");
        report.AppendLine("Generated: " + generatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        report.AppendLine();
        report.AppendLine("Total animals: " + animals.Length);
        report.AppendLine("Average age (years): " + (animals.Length == 0 ? "N/A" : animals.Average(animal => animal.Age).ToString("F2", CultureInfo.InvariantCulture)));
        report.AppendLine("Average recovery score: " + (animals.Length == 0 ? "N/A" : animals.Average(animal => animal.RecoveryScore).ToString("F2", CultureInfo.InvariantCulture)));
        report.AppendLine();
        report.AppendLine("Animals per status:");
        foreach (var boundary in new[] { 0, 20, 40, 60, 80 })
        {
            var status = AnimalRules.Classify(boundary).Status;
            report.AppendLine(status + ": " + animals.Count(animal => animal.Status == status));
        }
        report.AppendLine();
        report.AppendLine("Skipped malformed or duplicate lines: " + document.SkippedCount);
        report.AppendLine("Blank lines are ignored. Totals and averages include valid records only.");
        return report.ToString();
    }
}
