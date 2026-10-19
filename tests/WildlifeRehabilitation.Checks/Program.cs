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
Console.WriteLine($"{count} checks passed.");
