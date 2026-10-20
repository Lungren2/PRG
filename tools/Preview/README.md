# Form previews

On Windows, render the actual forms into PNGs without activating a window or injecting desktop input:

```powershell
dotnet run --project tools/Preview -- C:\Temp\PRG272-previews
```

The tool renders the shared-input register at normal and compact sizes, populated Add/edit fields, file-error feedback, summaries, and empty states. It exercises Add validation, duplicates, locked/read-only files, Search, row-click editing, the 79-to-80 boundary, Clear, automatic refresh, Delete choices, and matching on-screen/saved summaries using isolated data. Its optional Delete callback supplies Yes/No in tests; the normal app uses the native ID/name confirmation dialog.

These images show the solid fallback frame. Windows composites live acrylic outside `DrawToBitmap`, so the native blur is not included. A separate hidden-window probe reports whether Windows accepts the backdrop API.
