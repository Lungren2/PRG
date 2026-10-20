namespace WildlifeRehabilitation.Storage;

internal static class AtomicTextFile
{
    // Flushes a same-folder temporary file, publishes it atomically, and cleans up on failure.
    public static void Write(string path, string text, bool replaceExisting, string? backupPath = null)
    {
        var temporaryPath = Path.Combine(Path.GetDirectoryName(path)!, ".prg272-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(AnimalFileStore.Utf8.GetBytes(text));
                stream.Flush(true);
            }
            if (replaceExisting) File.Replace(temporaryPath, path, backupPath);
            else File.Move(temporaryPath, path);
        }
        finally
        {
            // A failed publication must not leave a temporary animal-data file behind.
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
