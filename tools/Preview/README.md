# Form previews

On Windows, render the actual forms into PNGs without activating a window or injecting desktop input:

```powershell
dotnet run --project tools/Preview -- C:\Temp\PRG272-previews
```

The tool renders the Main Menu, the register at normal and compact sizes, Update/Delete selection pages, Add, Update, and the summary. It checks all five menu routes and Back navigation, read-only column settings, refresh after Add/Update/Delete, missing and malformed files, Add validation and duplicate feedback, real Add and Update file writes, and summary generation using isolated fixture data.

These images show the solid fallback frame. Windows composites live acrylic outside `DrawToBitmap`, so the native blur is not included. A separate hidden-window probe reports whether Windows accepts the backdrop API.
