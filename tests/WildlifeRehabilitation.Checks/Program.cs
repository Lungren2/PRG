using System.Text;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

var count = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new Exception("FAIL: " + description);
    count++;
    Console.WriteLine("PASS: " + description);
}
var root = Path.Combine(Path.GetTempPath(), "PRG272-checks-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
Console.WriteLine("Fixtures: " + root);
var store = new AnimalFileStore(root);
var fresh = store.Load();
Check(fresh.Created && fresh.Records.Count >= 8 && fresh.Records.Select(x => x.Animal.Status).Distinct().Count() >= 4, "Missing file creates valid starters across statuses");
Check(!store.Load().Created, "Loading existing data does not reseed it");
Check(fresh.Records.Single(x => x.Animal.RecoveryScore == 39).Animal.Status == "Serious", "Score 39 loads as Serious");
var first = AnimalRules.Serialize(fresh.Records[0].Animal);
File.WriteAllText(store.DataPath, first + "\ninvalid\n" + first + "\n\n", new UTF8Encoding(false));
var mixed = store.Load();
Check(mixed.Records.Count == 1 && mixed.SkippedCount == 2, "Malformed and duplicate lines are counted; blank lines are ignored");
File.WriteAllText(store.DataPath, "");
Check(store.Load().Records.Count == 0 && !store.Load().Created, "An empty file remains empty");
File.WriteAllBytes(store.DataPath, [0xFF, 0xFE, 0xFF]);
try { store.Load(); Check(false, "Invalid UTF-8 rejected"); }
catch (DecoderFallbackException) { Check(true, "Invalid UTF-8 rejected"); }
File.WriteAllText(store.DataPath, first);
using (var locked = new FileStream(store.DataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
{
    try { store.Load(); Check(false, "Locked files produce an I/O error"); }
    catch (IOException) { Check(true, "Locked files produce an I/O error"); }
}
string[] expectedStatuses = ["Critical", "Critical", "Serious", "Serious", "Stable", "Stable", "Recovering", "Recovering", "Release-Ready", "Release-Ready"];
int[] boundaryScores = [0, 19, 20, 39, 40, 59, 60, 79, 80, 100];
Check(boundaryScores.Select(AnimalRules.Classify).Select(x => x.Status).SequenceEqual(expectedStatuses), "Every score boundary has the required classification");
string[] expectedHousing = ["Intensive Care Unit", "Intensive Care Unit", "High-Dependency Ward", "High-Dependency Ward", "Recovery Ward", "Recovery Ward", "Outdoor Enclosure", "Outdoor Enclosure", "Pre-Release Camp", "Pre-Release Camp"];
Check(boundaryScores.Select(AnimalRules.Classify).Select(x => x.HousingUnit).SequenceEqual(expectedHousing), "Tian's five recovery cases retain the required housing at every boundary");
foreach (var score in new[] { -1, 101 })
{
    try { AnimalRules.Classify(score); Check(false, "Out-of-range classification rejected"); }
    catch (ArgumentOutOfRangeException) { Check(true, "Out-of-range classification rejected"); }
}
foreach (var invalid in new[] {
    new AnimalDraft("WR-0000", "Name", "Species", "2", "50"),
    new AnimalDraft("WR-0011", "", "Species", "2", "50"),
    new AnimalDraft("WR-0011", "Bad|Name", "Species", "2", "50"),
    new AnimalDraft("WR-0011", "Name", "Species\nNext", "2", "50"),
    new AnimalDraft("WR-0011", "Name", "Species", "-1", "50"),
    new AnimalDraft("WR-0011", "Name", "Species", "2", "101"),
    new AnimalDraft("WR-0011", "Name", "Species", "2", "-1") })
    Check(!AnimalRules.TryCreate(invalid, out _, out _), "Invalid field input is rejected");
var added = new Animal("WR-0011", "Zoë", "Léopard", 2, 39);
File.WriteAllText(store.DataPath, first + "\nmalformed line", new UTF8Encoding(false));
var beforeAppend = File.ReadAllText(store.DataPath);
store.Append(added);
Check(File.ReadAllText(store.DataPath).StartsWith(beforeAppend + Environment.NewLine), "Append preserves existing raw lines and separates an unterminated final line");
Check(store.Load().Records.Last().Animal == added && store.Load().SkippedCount == 1, "UTF-8 added record round-trips with derived score-39 classification");
Check(File.ReadAllText(Path.Combine(root, "animals.backup.txt")) == beforeAppend, "Animal writes retain the previous file as a backup");
var afterAppend = File.ReadAllText(store.DataPath);
try { store.Append(added); Check(false, "Duplicate append rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == afterAppend, "Duplicate append preserves the data file"); }
File.AppendAllText(store.DataPath, "wr-0012|incomplete\n");
var withSkippedId = File.ReadAllText(store.DataPath);
try { store.Append(added with { Id = "WR-0012" }); Check(false, "ID on a malformed line rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == withSkippedId, "Tian's adapted ID scan reserves malformed-line IDs without changing the file"); }
File.WriteAllText(store.DataPath, afterAppend, new UTF8Encoding(false));
using (var writer = new FileStream(Path.Combine(root, ".animals.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
{
    try { new AnimalFileStore(root).Append(added with { Id = "WR-0012" }); Check(false, "Competing writer blocked"); }
    catch (IOException) { Check(File.ReadAllText(store.DataPath) == afterAppend, "Overlapping writers cannot overwrite the data file"); }
}
File.SetAttributes(store.DataPath, FileAttributes.ReadOnly);
try
{
    try { store.Append(added with { Id = "WR-0012" }); Check(false, "Read-only file rejected"); }
    catch (UnauthorizedAccessException) { Check(File.ReadAllText(store.DataPath) == afterAppend, "Read-only file failure preserves the original"); }
}
finally { File.SetAttributes(store.DataPath, FileAttributes.Normal); }
Check(!Directory.EnumerateFiles(root, ".prg272-*.tmp").Any(), "Failed writes clean up temporary data files");
var selected = store.Load().Records.Single(x => x.Animal.Id == added.Id);
var changed = added with { Name = "Zoë updated", RecoveryScore = 80 };
var untouchedPrefix = beforeAppend + Environment.NewLine;
store.Update(selected, changed);
Check(File.ReadAllText(store.DataPath) == untouchedPrefix + AnimalRules.Serialize(changed) + Environment.NewLine, "Update rewrites only the selected line and reclassifies status and housing");
var afterUpdate = File.ReadAllText(store.DataPath);
try { store.Update(selected, added with { Age = 8 }); Check(false, "Stale update rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == afterUpdate, "A stale editor cannot overwrite a newer change"); }
var current = store.Load().Records.Single(x => x.Animal.Id == added.Id);
try { store.Update(current, changed with { Id = "WR-0099" }); Check(false, "ID changes rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == afterUpdate, "Update keeps IDs immutable"); }
File.AppendAllText(store.DataPath, AnimalRules.Serialize(changed) + "\n");
var duplicated = File.ReadAllText(store.DataPath);
try { store.Update(current, changed with { Age = 8 }); Check(false, "Ambiguous update rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == duplicated, "Duplicate IDs cannot cause an update to target the wrong row"); }
try { store.Delete(current); Check(false, "Ambiguous delete rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == duplicated, "Delete cannot remove multiple records sharing the selected ID"); }
File.WriteAllText(store.DataPath, afterUpdate + " wr-0011|incomplete\n");
var duplicateSkippedId = File.ReadAllText(store.DataPath);
try { store.Delete(current); Check(false, "Malformed duplicate delete rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == duplicateSkippedId, "Delete preserves valid and malformed lines when their normalized IDs conflict"); }
File.WriteAllText(store.DataPath, "new malformed line\r\n" + afterUpdate);
store.Update(current, changed with { Age = 8 });
Check(File.ReadAllText(store.DataPath).StartsWith("new malformed line\r\n" + untouchedPrefix), "Update finds the original record after another line is inserted, preserving other line endings");
var beforeDelete = File.ReadAllText(store.DataPath);
try { store.Delete(current); Check(false, "Stale delete rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == beforeDelete, "A stale selection cannot delete a changed record"); }
var toDelete = store.Load().Records.Single(x => x.Animal.Id == added.Id);
using (var locked = new FileStream(store.DataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
{
    try { store.Delete(toDelete); Check(false, "Locked delete rejected"); }
    catch (IOException) { Check(true, "Delete reports a locked file without changing it"); }
}
Check(File.ReadAllText(store.DataPath) == beforeDelete, "Locked delete preserves file contents");
store.Delete(toDelete);
Check(File.ReadAllText(store.DataPath) == "new malformed line\r\n" + untouchedPrefix, "Delete removes only the selected record and preserves skipped lines");
Check(File.ReadAllText(Path.Combine(root, "animals.backup.txt")) == beforeDelete, "Delete retains the previous data file as a recovery backup");
var afterDelete = File.ReadAllText(store.DataPath);
try { store.Delete(toDelete); Check(false, "Missing target rejected"); }
catch (AnimalConflictException) { Check(File.ReadAllText(store.DataPath) == afterDelete, "Deleting an already-removed record cannot delete a different animal"); }
store.Delete(store.Load().Records.Single());
Check(store.Load().Records.Count == 0 && !store.Load().Created, "Deleting the last valid animal leaves an empty view without restoring starters");
var critical = new Animal("WR-0020", "One", "Species", 2, 0);
var ready = new Animal("WR-0021", "Two", "Species", 8, 100);
File.WriteAllText(store.DataPath, AnimalRules.Serialize(critical) + "\nmalformed\n" + AnimalRules.Serialize(ready) + "\n" + AnimalRules.Serialize(critical));
var report = AnimalSummary.Create(store.Load(), new DateTime(2026, 10, 19, 12, 30, 0));
Check(report.Contains("Total animals: 2") && report.Contains("Average age (years): 5.00") && report.Contains("Average recovery score: 50.00"), "Summary totals and averages include only valid unique animals");
Check(report.Contains("Critical: 1") && report.Contains("Serious: 0") && report.Contains("Stable: 0") && report.Contains("Recovering: 0") && report.Contains("Release-Ready: 1"), "Summary includes all five status counts, including zeroes");
Check(report.Contains("Skipped malformed or duplicate lines: 2") && report.Contains("Generated: 2026-10-19 12:30"), "Summary reports skipped lines and its generation time");
store.SaveSummary(report);
Check(File.ReadAllText(store.SummaryPath, Encoding.UTF8) == report, "Saved UTF-8 summary matches the generated report");
File.WriteAllText(store.DataPath, "");
var emptyReport = AnimalSummary.Create(store.Load(), DateTime.Now);
Check(emptyReport.Contains("Total animals: 0") && emptyReport.Contains("Average age (years): N/A") && emptyReport.Contains("Average recovery score: N/A"), "An empty summary has no division-by-zero or fabricated averages");
using (var lockedSummary = new FileStream(store.SummaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
{
    try { store.SaveSummary(emptyReport); Check(false, "Locked summary rejected"); }
    catch (IOException) { Check(true, "Locked summary reports a save failure"); }
}
Check(File.ReadAllText(store.SummaryPath) == report, "A failed summary save preserves the existing report");
store.SaveSummary(emptyReport);
Check(File.ReadAllText(store.SummaryPath) == emptyReport, "A regenerated report replaces the previous summary");
Check(File.ReadAllText(store.DataPath) == "", "Summary generation never changes animal data");
var statusFixtures = new[] { 0, 20, 40, 60, 80 }.Select((score, index) =>
    new Animal($"WR-{index + 30:0000}", "Animal", "Species", 3, score));
File.WriteAllText(store.DataPath, string.Join("\n", statusFixtures.Select(AnimalRules.Serialize)));
var allStatusReport = AnimalSummary.Create(store.Load(), DateTime.Now);
Check(new[] { "Critical: 1", "Serious: 1", "Stable: 1", "Recovering: 1", "Release-Ready: 1" }.All(allStatusReport.Contains), "All five of Aseza's named counters count their matching animals");
File.WriteAllText(store.DataPath, AnimalRules.Serialize(critical with { Age = int.MaxValue }) + "\n" + AnimalRules.Serialize(ready with { Age = int.MaxValue }));
var largeAgeReport = AnimalSummary.Create(store.Load(), DateTime.Now);
Check(largeAgeReport.Contains("Average age (years): 2147483647.00"), "Summary totals cannot overflow when valid animal ages exceed an integer sum");
Console.WriteLine($"{count} checks passed.");
