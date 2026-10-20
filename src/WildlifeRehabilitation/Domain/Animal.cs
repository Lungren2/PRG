namespace WildlifeRehabilitation.Domain;

// Stores the five entered fields and derives both calculated fields through the shared classifier.
public sealed record Animal(string Id, string Name, string Species, int Age, int RecoveryScore)
{
    public string Status => AnimalRules.Classify(RecoveryScore).Status;
    public string HousingUnit => AnimalRules.Classify(RecoveryScore).HousingUnit;
}

// Carries unvalidated textbox/file fields to the single validation method.
public sealed record AnimalDraft(string Id, string Name, string Species, string Age, string RecoveryScore);
