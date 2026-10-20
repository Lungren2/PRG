namespace WildlifeRehabilitation.Storage;

// Reports duplicate IDs or stale selections as a recoverable validation conflict.
public sealed class AnimalConflictException(string message) : Exception(message);
