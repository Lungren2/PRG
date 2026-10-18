namespace WildlifeRehabilitation.Domain;

public sealed record Animal(string Id, string Name, string Species, int Age, int RecoveryScore)
{
    public string Status => AnimalRules.Classify(RecoveryScore).Status;
    public string HousingUnit => AnimalRules.Classify(RecoveryScore).HousingUnit;
}

public sealed record AnimalDraft(string Id, string Name, string Species, string Age, string RecoveryScore);
