using System.Globalization;
using System.Text.RegularExpressions;

namespace WildlifeRehabilitation.Domain;

public static partial class AnimalRules
{
    [GeneratedRegex(@"^WR-[0-9]{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdPattern();

    public static (string Status, string HousingUnit) Classify(int score) => score switch
    {
        >= 0 and <= 19 => ("Critical", "Intensive Care Unit"),
        >= 20 and <= 39 => ("Serious", "High-Dependency Ward"),
        >= 40 and <= 59 => ("Stable", "Recovery Ward"),
        >= 60 and <= 79 => ("Recovering", "Outdoor Enclosure"),
        >= 80 and <= 100 => ("Release-Ready", "Pre-Release Camp"),
        _ => throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 0 and 100.")
    };

    public static bool TryCreate(AnimalDraft draft, out Animal? animal, out Dictionary<string, string> errors)
    {
        errors = new Dictionary<string, string>();
        var id = draft.Id.Trim().ToUpperInvariant();
        if (!IdPattern().IsMatch(id) || id == "WR-0000")
            errors[nameof(draft.Id)] = "Use an ID from WR-0001 to WR-9999.";
        ValidateText(draft.Name, nameof(draft.Name), errors);
        ValidateText(draft.Species, nameof(draft.Species), errors);
        if (!int.TryParse(draft.Age.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var age))
            errors[nameof(draft.Age)] = "Age must be a nonnegative whole number.";
        if (!int.TryParse(draft.RecoveryScore.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var score) || score > 100)
            errors[nameof(draft.RecoveryScore)] = "Score must be a whole number from 0 to 100.";
        animal = errors.Count == 0 ? new Animal(id, draft.Name.Trim(), draft.Species.Trim(), age, score) : null;
        return animal != null;
    }

    private static void ValidateText(string value, string field, Dictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 80 || value.Contains('|') || value.Any(char.IsControl))
            errors[field] = $"{field} is required, up to 80 characters, with no pipes or control characters.";
    }

    public static string Serialize(Animal animal) => string.Join('|', animal.Id, animal.Name, animal.Species,
        animal.Age.ToString(CultureInfo.InvariantCulture), animal.RecoveryScore.ToString(CultureInfo.InvariantCulture),
        animal.Status, animal.HousingUnit);

    public static bool TryParse(string line, out Animal? animal)
    {
        animal = null;
        var fields = line.Split('|');
        if (fields.Length != 7 || !TryCreate(new AnimalDraft(fields[0], fields[1], fields[2], fields[3], fields[4]), out var parsed, out _))
            return false;
        if (!string.Equals(fields[5].Trim(), parsed!.Status, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(fields[6].Trim(), parsed.HousingUnit, StringComparison.OrdinalIgnoreCase))
            return false;
        animal = parsed;
        return true;
    }
}
