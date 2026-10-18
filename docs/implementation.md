# Integration decisions

The application uses .NET 8 Windows Forms. The original projects are preserved in Git history and in the existing local import folders. The active solution replaces their UI and file code.

## Shared contract

- One immutable `Animal` record has ID, name, species, whole-year age, and recovery score. Status and housing are computed by `AnimalRules.Classify`.
- IDs use `WR-0001` through `WR-9999`. Names and species are required, at most 80 characters, and cannot contain pipes or control characters. Age is a nonnegative integer. Score is an integer from 0 to 100.
- Score ranges are inclusive: 0–19 Critical, 20–39 Serious, 40–59 Stable, 60–79 Recovering, and 80–100 Release-Ready.
- UTF-8 records use `ID|Name|Species|Age|RecoveryScore|Status|HousingUnit`. Both forms use the same parser, validation, and classification.
- `AppContext.BaseDirectory` owns `animals.txt` and `summary.txt`, regardless of the current working directory. A missing animal file is created with ten starter records across all five statuses. An existing empty file stays empty.
- Blank lines are ignored. Malformed records, inconsistent classification, and repeated IDs are skipped and counted. Mutations preserve their raw lines. A stale or ambiguous selection cannot silently overwrite another record.
- File operations fail once with actionable feedback. No automatic retry occurs. Inputs remain open after failed saves. A successful mutation refreshes the grid.
- Application writers acquire an exclusive sidecar file handle. Process exit releases the handle. Writes publish through a same-directory temporary UTF-8 file and atomic replacement, with `animals.backup.txt` retaining the previous animal file. External editors must close the file before changes. A lock or permission error leaves the current file unchanged.

## UI source and implementation contract

The precedent is Microsoft's small-record DataGridView sample, which supports selecting a full record and using explicit Add and Delete actions. It fits this desktop assignment more closely than a database management shell or a web dashboard.

- Source: https://github.com/dotnet/docs-desktop
- Commit: `1c487f4e64ba00769475c03aa8a65f05f137c13d`
- Path: `dotnet-desktop-guide/samples/snippets/csharp/VS_Snippets_Winforms/System.Windows.Forms.DataGridViewSimpleUnbound/CS/simpleunbound.cs`
- SHA-256: `9BCE72EFB6737453C6B0420C08E4E45B7A4B99AA0063050E7675AA6AAAD6C4E9`
- Code license: MIT, retained in `THIRD-PARTY-NOTICES.txt`.
- Mapping: the upstream file was copied to `src/WildlifeRehabilitation/MainForm.cs` before adaptation. `SetupDataGridView` retains the unbound rows, full-row single selection, hidden row headers, docked grid, and explicit action controls.
- Assignment changes: seven animal columns replace song columns; the grid is read-only; file loading replaces inline samples; Add and Update use a modal editor; Delete confirms before writing; Summary opens a modal report. Native keyboard focus and grid scrolling remain. A resizable layout replaces fixed coordinates so the seven required fields stay usable.

The blank WinForms foundation was committed before feature implementation. Feature branches are merged in view, add, update, delete, summary order. This order establishes the shared model and loading contract before writes.

## Desktop appearance

The existing grid and modal workflow use a shared light theme, Segoe UI typography, a teal primary action, and readable status colors. Standard Windows controls preserve keyboard access. High-contrast mode uses system colors.

`WindowsBackdropForm` applies the documented Windows 11 desktop acrylic backdrop to the title bar and outer frame. Content surfaces stay opaque. Windows 10, unsupported APIs, and offscreen child-form previews use a soft gradient frame. High-contrast startup uses system colors. Window opacity and color-key transparency are never changed.

The native integration uses [DWM_SYSTEMBACKDROP_TYPE](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type) and [DwmExtendFrameIntoClientArea](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmextendframeintoclientarea). The API requires Windows 11 build 22621 or newer. Backdrop and frame HRESULTs are checked before enabling the glass background. The optional light title-bar hint can fail without affecting the backdrop.

WinBlur was evaluated for reuse. It is archived and its implementation sets a black background unconditionally, ignores backdrop failures, and extends the frame by an arbitrary large height. A small adapter to Microsoft's published APIs avoids that dependency and adds the required version, high-contrast, and failure handling. No third-party blur engine is included.

## Recovery

Close programs that lock a data file and retry. Run the application from a writable folder. To restore the previous animal file, close the application, copy `animals.backup.txt` over `animals.txt`, then reopen it. Regenerate `summary.txt` through Summary report.
