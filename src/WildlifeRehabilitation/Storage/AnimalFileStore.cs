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
    public string DirectoryPath => Path.GetDirectoryName(DataPath)!;

    public AnimalFileStore(string directory) => DataPath = Path.Combine(Path.GetFullPath(directory), "animals.txt");

    public AnimalDocument Load()
    {
        var created = false;
        if (!File.Exists(DataPath))
        {
            using var seed = typeof(AnimalFileStore).Assembly.GetManifestResourceStream("WildlifeRehabilitation.Data.animals.seed.txt")!;
            using var reader = new StreamReader(seed, Utf8);
            var content = reader.ReadToEnd();
            using var file = new FileStream(DataPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var bytes = Utf8.GetBytes(content);
            file.Write(bytes);
            file.Flush(true);
            created = true;
        }
        return ReadDocument(created);
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
