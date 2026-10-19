using System.Text;
using System.Text.RegularExpressions;
using WildlifeRehabilitation.Domain;

namespace WildlifeRehabilitation.Storage;

public sealed record StoredAnimal(Animal Animal, string RawLine, int LineIndex);
public sealed record FileLine(string Content, string Ending);
public sealed record AnimalDocument(IReadOnlyList<FileLine> Lines, IReadOnlyList<StoredAnimal> Records, int SkippedCount, bool Created);

public sealed partial class AnimalFileStore
{
    internal static readonly UTF8Encoding Utf8 = new(false, true);
    public string DataPath { get; }
    public string SummaryPath => Path.Combine(DirectoryPath, "summary.txt");
    public string DirectoryPath => Path.GetDirectoryName(DataPath)!;

    public AnimalFileStore(string directory) => DataPath = Path.Combine(Path.GetFullPath(directory), "animals.txt");

    public AnimalDocument Load()
    {
        var created = false;
        if (!File.Exists(DataPath))
        {
            using var guard = AcquireWriter();
            if (File.Exists(DataPath)) return ReadDocument();
            using var seed = typeof(AnimalFileStore).Assembly.GetManifestResourceStream("WildlifeRehabilitation.Data.animals.seed.txt")!;
            using var reader = new StreamReader(seed, Utf8);
            var content = reader.ReadToEnd();
            AtomicTextFile.Write(DataPath, content, replaceExisting: false);
            created = true;
        }
        return ReadDocument(created);
    }

    private FileStream AcquireWriter() => new(Path.Combine(DirectoryPath, ".animals.lock"),
        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);

    private static string RawId(FileLine line) => line.Content.Split('|')[0].Trim().ToUpperInvariant();

    public string SuggestId()
    {
        var used = Load().Lines.Select(RawId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var number = 1; number <= 9999; number++)
        {
            var id = $"WR-{number:0000}";
            if (!used.Contains(id)) return id;
        }
        throw new AnimalConflictException("All IDs from WR-0001 to WR-9999 are in use.");
    }

    public void Append(Animal animal)
    {
        animal = ValidateForSave(animal);
        using var guard = AcquireWriter();
        var document = ReadDocument();
        // Adapted from Tian's Classes.TextWriter duplicate-ID scan.
        // Check raw lines too, so an invalid record cannot give its ID to a new animal.
        bool exists = false;
        foreach (var line in document.Lines)
        {
            string[] ids = line.Content.Split('|');
            if (ids[0].Trim().Equals(animal.Id, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }
        if (exists)
            throw new AnimalConflictException($"ID {animal.Id} already exists, including in a skipped line. Use another ID.");
        var original = JoinLines(document.Lines);
        var separator = original.Length > 0 && !original.EndsWith('\n') && !original.EndsWith('\r') ? Environment.NewLine : "";
        SaveAnimals(original + separator + AnimalRules.Serialize(animal) + Environment.NewLine);
    }

    public void Update(StoredAnimal expected, Animal replacement)
    {
        replacement = ValidateForSave(replacement);
        if (replacement.Id != expected.Animal.Id)
            throw new AnimalConflictException("An animal ID cannot be changed during an update.");
        using var guard = AcquireWriter();
        var document = ReadDocument();
        var target = FindCurrentRecord(document, expected);
        var lines = document.Lines.ToArray();
        lines[target.LineIndex] = lines[target.LineIndex] with { Content = AnimalRules.Serialize(replacement) };
        SaveAnimals(JoinLines(lines));
    }

    public void Delete(StoredAnimal expected)
    {
        using var guard = AcquireWriter();
        var document = ReadDocument();
        var target = FindCurrentRecord(document, expected);
        SaveAnimals(JoinLines(document.Lines.Where((_, index) => index != target.LineIndex)));
    }

    private static StoredAnimal FindCurrentRecord(AnimalDocument document, StoredAnimal expected)
    {
        if (document.Lines.Count(line => RawId(line) == expected.Animal.Id) != 1)
            throw new AnimalConflictException("The record is missing or its ID appears more than once. Cancel, repair duplicate IDs if necessary, and refresh.");
        var current = document.Records.SingleOrDefault(record => record.Animal.Id == expected.Animal.Id);
        if (current is null || current.RawLine != expected.RawLine)
            throw new AnimalConflictException("This record changed after it was loaded. Cancel and refresh before trying again.");
        return current;
    }

    private static Animal ValidateForSave(Animal animal)
    {
        var draft = new AnimalDraft(animal.Id, animal.Name, animal.Species,
            animal.Age.ToString(System.Globalization.CultureInfo.InvariantCulture),
            animal.RecoveryScore.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (!AnimalRules.TryCreate(draft, out var validated, out var errors))
            throw new ArgumentException(string.Join(" ", errors.Values));
        return validated!;
    }

    private static string JoinLines(IEnumerable<FileLine> lines) => string.Concat(lines.Select(line => line.Content + line.Ending));

    private void SaveAnimals(string text) => AtomicTextFile.Write(DataPath, text,
        replaceExisting: true, Path.Combine(DirectoryPath, "animals.backup.txt"));

    public void SaveSummary(string report)
    {
        using var guard = AcquireWriter();
        AtomicTextFile.Write(SummaryPath, report, replaceExisting: File.Exists(SummaryPath));
    }

    private AnimalDocument ReadDocument(bool created = false)
    {
        using var file = new FileStream(DataPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        using var bytes = new MemoryStream();
        file.CopyTo(bytes);
        var text = Utf8.GetString(bytes.ToArray()).TrimStart('\uFEFF');
        var lines = SplitLines(text);
        var records = new List<StoredAnimal>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skipped = 0;
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index].Content;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!AnimalRules.TryParse(line, out var animal) || !ids.Add(animal!.Id))
            {
                skipped++;
                continue;
            }
            records.Add(new StoredAnimal(animal!, line, index));
        }
        return new AnimalDocument(lines, records, skipped, created);
    }

    [GeneratedRegex(@"([^\r\n]*)(\r\n|\r|\n|$)")]
    private static partial Regex LinePattern();

    private static List<FileLine> SplitLines(string text) => LinePattern().Matches(text)
        .Where(match => match.Length != 0)
        .Select(match => new FileLine(match.Groups[1].Value, match.Groups[2].Value)).ToList();
}
