using System.Globalization;
using System.Text;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation.Domain;

public static class AnimalSummary
{
    // Builds the brief's exact report layout from valid, unique records only.
    public static string Create(AnimalDocument document, DateTime generatedAt)
    {
        // Adapted from Aseza Patiwe's frmSummaryReport.btnSummary_Click totals and five status counters.
        // Original: reference/delete-summary/frmSummaryReport.cs. Load has already validated records.
        int totalAnimals = document.Records.Count;
        long totalAge = 0;
        long totalScore = 0;
        int criticalCount = 0;
        int seriousCount = 0;
        int stableCount = 0;
        int recoveringCount = 0;
        int releaseReadyCount = 0;
        foreach (var record in document.Records)
        {
            var animal = record.Animal;
            totalAge += animal.Age;
            totalScore += animal.RecoveryScore;
            string status = animal.Status;
            if (status == "Critical") criticalCount++;
            else if (status == "Serious") seriousCount++;
            else if (status == "Stable") stableCount++;
            else if (status == "Recovering") recoveringCount++;
            else if (status == "Release-Ready") releaseReadyCount++;
        }
        var report = new StringBuilder();
        report.AppendLine("BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE");
        report.AppendLine("ANIMAL SUMMARY REPORT");
        report.AppendLine("Generated: " + generatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        report.AppendLine("----------------------------------------");
        if (totalAnimals == 0)
        {
            report.AppendLine("Total animals:".PadRight(26) + "0");
            report.AppendLine("Average age (years):".PadRight(26) + "N/A");
            report.AppendLine("Average recovery score:".PadRight(26) + "N/A");
        }
        else
        {
            double avgAge = (double)totalAge / totalAnimals;
            double avgScore = (double)totalScore / totalAnimals;
            report.AppendLine("Total animals:".PadRight(26) + totalAnimals.ToString(CultureInfo.InvariantCulture));
            report.AppendLine("Average age (years):".PadRight(26) + avgAge.ToString("F2", CultureInfo.InvariantCulture));
            report.AppendLine("Average recovery score:".PadRight(26) + avgScore.ToString("F2", CultureInfo.InvariantCulture));
        }
        report.AppendLine("----------------------------------------");
        report.AppendLine("Animals per status:");
        report.AppendLine("Critical:".PadRight(26) + criticalCount.ToString(CultureInfo.InvariantCulture));
        report.AppendLine("Serious:".PadRight(26) + seriousCount.ToString(CultureInfo.InvariantCulture));
        report.AppendLine("Stable:".PadRight(26) + stableCount.ToString(CultureInfo.InvariantCulture));
        report.AppendLine("Recovering:".PadRight(26) + recoveringCount.ToString(CultureInfo.InvariantCulture));
        report.AppendLine("Release-Ready:".PadRight(26) + releaseReadyCount.ToString(CultureInfo.InvariantCulture));
        return report.ToString();
    }
}
