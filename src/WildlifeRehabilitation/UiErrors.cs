using System.Text;

namespace WildlifeRehabilitation;

internal static class UiErrors
{
    // Identifies expected storage failures that the UI can report without terminating the app.
    public static bool IsFileError(Exception exception) => exception is IOException or UnauthorizedAccessException or DecoderFallbackException;

    // Translates file errors into one friendly message with a practical recovery action.
    public static string Describe(Exception exception) => exception switch
    {
        UnauthorizedAccessException => "The folder or file is read-only, or you do not have permission. Move the application to a writable folder, or ask for access, then try again.",
        DecoderFallbackException => "animals.txt contains invalid UTF-8. Save it as UTF-8 in a text editor, then refresh. The file has not been changed.",
        FileNotFoundException => "The data file was removed. Refresh to create an empty file, then try again.",
        _ => "The file could not be accessed. Close any program that has locked it, check that the folder is available, then try again. " + exception.Message
    };
}
