using System.Globalization;
using System.Text.RegularExpressions;

namespace WildlifeRehabilitation.Domain;

public static partial class AnimalRules
{
    // Builds the exact WR- plus four digits ID check required by the brief.
    [GeneratedRegex(@"^WR-[0-9]{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdPattern();

    // Adapted from Tian's Classes.TextWriter switch. Original: reference/tian-add/Classes.cs.
    // Add and Update share this one copy; the Serious range now includes score 39.
    public static (string Status, string HousingUnit) Classify(int animalRecoveryScore)
    {
        string housingUnit;
        string status;
        switch (animalRecoveryScore)
        {
            case int a when a >= 0 && a <= 19:
                housingUnit = "Intensive Care Unit";
                status = "Critical";
                break;
            case int a when a >= 20 && a <= 39:
                housingUnit = "High-Dependency Ward";
                status = "Serious";
                break;
            case int a when a >= 40 && a <= 59:
                housingUnit = "Recovery Ward";
                status = "Stable";
                break;
            case int a when a >= 60 && a <= 79:
                housingUnit = "Outdoor Enclosure";
                status = "Recovering";
                break;
            case int a when a >= 80 && a <= 100:
                housingUnit = "Pre-Release Camp";
                status = "Release-Ready";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(animalRecoveryScore), "Score must be between 0 and 100.");
        }
        return (status, housingUnit);
    }

    // Trims every input and applies the same field rules for Add, Update, and file loading.
    public static bool TryCreate(AnimalDraft draft, out Animal? animal, out Dictionary<string, string> errors)
    {
        errors = new Dictionary<string, string>();
        var id = draft.Id.Trim().ToUpperInvariant();
        if (!IdPattern().IsMatch(id))
            errors[nameof(draft.Id)] = "Animal ID must be WR- followed by exactly four digits.";
        ValidateText(draft.Name, nameof(draft.Name), errors);
        ValidateText(draft.Species, nameof(draft.Species), errors);
        if (!int.TryParse(draft.Age.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var age) || age > 100)
            errors[nameof(draft.Age)] = "Age must be a whole number between 0 and 100.";
        if (!int.TryParse(draft.RecoveryScore.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var score) || score > 100)
            errors[nameof(draft.RecoveryScore)] = "Recovery Score must be a whole number between 0 and 100.";
        animal = errors.Count == 0 ? new Animal(id, draft.Name.Trim(), draft.Species.Trim(), age, score) : null;
        return animal != null;
    }

    // Rejects empty, overlong, or multiline names/species before they reach the text file.
    private static void ValidateText(string value, string field, Dictionary<string, string> errors)
    {
        value = value.Trim();
        if (value.Length is < 1 or > 30 || value.Contains('|') || value.Any(char.IsControl))
            errors[field] = $"{field} must have 1-30 characters and cannot contain pipes or control characters.";
    }

    // Writes one record in the seven-field, invariant UTF-8 text-file format.
    public static string Serialize(Animal animal) => string.Join('|', animal.Id, animal.Name, animal.Species,
        animal.Age.ToString(CultureInfo.InvariantCulture), animal.RecoveryScore.ToString(CultureInfo.InvariantCulture),
        animal.Status, animal.HousingUnit);

    // Accepts only seven valid fields with the required calculated status and housing.
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
