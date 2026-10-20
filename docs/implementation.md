# Integration decisions

The application uses .NET 8 Windows Forms. The original projects are preserved in Git history and in the existing local import folders. The active app adapts Tian Heydenrych's Add field handling, duplicate-ID scan, and classification switch, plus Aseza Patiwe's Delete flow and Summary counters and averages. The [Add code map](add-animal.md) and [Delete and Summary code map](delete-summary.md) record source credit and integration changes. Shared validation, storage infrastructure, and the current UI were added during consolidation. Feature ownership is listed in the README and does not reassign Git authorship.

## Shared contract

- One immutable `Animal` record has ID, name, species, whole-year age, and recovery score. Status and housing are computed by `AnimalRules.Classify`.
- IDs use `WR-0000` through `WR-9999`, case-normalized before validation. Names and species are trimmed, required, 1-30 characters, and cannot contain pipes or control characters. Age and score are whole numbers from 0 to 100.
- Score ranges are inclusive: 0–19 Critical, 20–39 Serious, 40–59 Stable, 60–79 Recovering, and 80–100 Release-Ready.
- UTF-8 records use `ID|Name|Species|Age|RecoveryScore|Status|HousingUnit`. Both forms use the same parser, validation, and classification.
- `AppContext.BaseDirectory` owns `animals.txt` and `summary.txt`, regardless of the current working directory. The project-root `animals.txt` contributed with Nkosinathi's View feature contains ten starter records across all five statuses and is copied beside the executable with `CopyToOutputDirectory=PreserveNewest`, Visual Studio's Copy if newer setting. At runtime a missing file is created empty; an existing empty file stays empty.
- Blank and malformed lines, inconsistent classification, and repeated IDs are skipped and counted once in the register's load status. Mutations preserve their raw lines. A stale or ambiguous selection cannot silently overwrite another record.
- File operations fail once with actionable feedback. No automatic retry occurs. Inputs remain open after failed saves. A successful mutation refreshes the grid.
- Add and Update use the same `AnimalEditorPanel` inputs. Clicking a row or searching an exact case-insensitive ID enters edit mode with a read-only ID, Add disabled, and Save Changes enabled. Clear discards the draft. Successful saves show calculated status/housing, clear inputs, and return to Add mode.
- Summary text uses the required title, timestamp, two 40-character separators, aligned totals/averages, and five status lines. Values start in column 27, matching the supplied example. Invariant formatting keeps decimal full stops; empty averages are `N/A`. Skipped counts appear in the register, not as extra summary lines.
- Application writers acquire an exclusive sidecar file handle. Process exit releases the handle. Writes publish through a same-directory temporary UTF-8 file and atomic replacement, with `animals.backup.txt` retaining the previous animal file. External editors must close the file before changes. A lock or permission error leaves the current file unchanged.

## UI source and implementation contract

The precedent is Microsoft's small-record DataGridView sample, which supports selecting a full record and using explicit Add and Delete actions. It fits this desktop assignment more closely than a database management shell or a web dashboard.

- Source: https://github.com/dotnet/docs-desktop
- Commit: `1c487f4e64ba00769475c03aa8a65f05f137c13d`
- Path: `dotnet-desktop-guide/samples/snippets/csharp/VS_Snippets_Winforms/System.Windows.Forms.DataGridViewSimpleUnbound/CS/simpleunbound.cs`
- SHA-256: `9BCE72EFB6737453C6B0420C08E4E45B7A4B99AA0063050E7675AA6AAAD6C4E9`
- Code license: MIT, retained in `THIRD-PARTY-NOTICES.txt`.
- Mapping: the upstream file was copied to `src/WildlifeRehabilitation/MainForm.cs` before adaptation. `SetupDataGridView` retains the unbound rows, full-row single selection, hidden row headers, docked grid, and explicit action controls.
- Assignment changes: seven animal columns replace song columns; the grid is read-only and keeps file order; file loading replaces inline samples; Add and Update share inputs above the grid; Delete confirms before writing; Summary opens a modal report. Native keyboard focus and grid scrolling remain. A resizable layout replaces fixed coordinates so the seven required fields stay usable.


The register opens at startup, as required by the brief. Nkosinathi's Main Menu artwork and five navigation buttons remain accessible from the register; View-only pages retain the shared loader, file order, skipped counts, and menu-return navigation. Add and Update pages reuse the same five-field editor.

## Desktop appearance

The grid, shared editor, and modal report use a light theme, Segoe UI typography, a teal Add action, and readable status colors. Standard Windows controls preserve keyboard access. High-contrast mode uses system colors.

`WindowsBackdropForm` applies the documented Windows 11 desktop acrylic backdrop to the title bar and outer frame. Content surfaces stay opaque. Windows 10, unsupported APIs, and offscreen child-form previews use a soft gradient frame. High-contrast startup uses system colors. Window opacity and color-key transparency are never changed.

Frame extension uses the form's reserved padding as positive margins. Negative margins extend glass across the entire client area, causing GDI-rendered labels, button captions, and grid text to lose opacity during desktop composition. `DrawToBitmap` does not reproduce that compositor effect, so bitmap previews alone cannot verify live acrylic text rendering.

The native integration uses [DWM_SYSTEMBACKDROP_TYPE](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type) and [DwmExtendFrameIntoClientArea](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmextendframeintoclientarea). The API requires Windows 11 build 22621 or newer. Backdrop and frame HRESULTs are checked before enabling the glass background. The optional light title-bar hint can fail without affecting the backdrop.

WinBlur was evaluated for reuse. It is archived and its implementation sets a black background unconditionally, ignores backdrop failures, and extends the frame by an arbitrary large height. A small adapter to Microsoft's published APIs avoids that dependency and adds the required version, high-contrast, and failure handling. No third-party blur engine is included.

## Recovery

Close programs that lock a data file and retry. Run the application from a writable folder. To restore the previous animal file, close the application, copy `animals.backup.txt` over `animals.txt`, then reopen it. Regenerate `summary.txt` through Summary report.
