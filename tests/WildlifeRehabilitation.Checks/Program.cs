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
Console.WriteLine($"{count} checks passed.");
