# Form previews

On Windows, render the actual forms into PNGs without activating a window or injecting desktop input:

```powershell
dotnet run --project tools/Preview -- C:\Temp\PRG272-previews
```

The tool renders the register at normal and compact sizes, the editor, and the summary. It checks initial selection actions, the editor's Save handler, and summary generation using isolated fixture data.

These images show the solid fallback frame. Windows composites live acrylic outside `DrawToBitmap`, so the native blur is not included. A separate hidden-window probe reports whether Windows accepts the backdrop API.
