# PRG272

One .NET 8 Windows Forms application for the Baobab Ridge Wildlife Rehabilitation Centre. Add, view, update, delete, and summary reporting share one animal model, validator, classifier, and UTF-8 file store.

## Run

Use Windows with the .NET 8 SDK. A newer SDK also works if the .NET 8 Desktop Runtime is installed. Keep the checkout in a writable folder.

Double-click `Run.cmd`. It checks prerequisites, builds the solution, and opens Animal records. Build failures appear in the console and in `build.log`. The launcher does not overwrite animal data during a rebuild.

Alternatively, open `PRG272.sln` in Visual Studio with the .NET desktop development workload, or run:

```powershell
dotnet run --project src/WildlifeRehabilitation
```

The Release executable is `src/WildlifeRehabilitation/bin/Release/net8.0-windows/WildlifeRehabilitation.exe`. After building it, you can launch it directly.

## Use the application

- Add animal opens a modal editor with an available ID suggestion. Correct any marked fields and select Save. Failed saves leave your inputs in place.
- Select a row and choose Edit selected, or double-click the row. You can also enter an ID in Find ID and press Enter to select it. IDs stay fixed during an update.
- Delete selected asks for confirmation and defaults to No. Successful changes refresh the grid and keep the main window open.
- Summary report shows totals, average age, average recovery score, and all five status counts. It saves the same report to `summary.txt`. If saving fails, the report stays visible with an error message and a Generate again action.
- Refresh reloads the file and displays the number of valid animals and skipped lines. The grid is read-only. Empty data disables Edit and Delete.

Keyboard access includes Tab and Shift+Tab, the button access keys, Enter to find an ID or save an editor, Escape to cancel an editor or close a report, and Delete in the grid to request deletion.

## Data contract

`animals.txt`, `summary.txt`, and the previous animal file, `animals.backup.txt`, live beside the executable. The current working directory does not affect these paths. Runtime files and build output are excluded from Git.

A missing animal file is created with ten starter animals across all five statuses. An existing empty file remains empty. Seed data is embedded in the application and does not overwrite existing files when building or starting it.

Each UTF-8 record has seven pipe-separated fields:

```text
WR-0001|Thabo|African Penguin|4|85|Release-Ready|Pre-Release Camp
```

IDs range from `WR-0001` to `WR-9999`. Name and species are required, up to 80 characters, with no pipes or control characters. Age must be a nonnegative whole number. Recovery score must be a whole number from 0 to 100. Names can contain Unicode characters.

| Score | Status | Housing unit |
| --- | --- | --- |
| 0 to 19 | Critical | Intensive Care Unit |
| 20 to 39 | Serious | High-Dependency Ward |
| 40 to 59 | Stable | Recovery Ward |
| 60 to 79 | Recovering | Outdoor Enclosure |
| 80 to 100 | Release-Ready | Pre-Release Camp |

Blank lines are ignored. Malformed records, invalid field values, inconsistent classifications, and repeated IDs are skipped and counted. Totals and averages use valid unique records only. Updates and deletions preserve unrelated raw lines, including malformed ones and their line endings. Repair malformed or duplicate records in a UTF-8 text editor, close the file, then select Refresh.

## File errors and recovery

The application reports locked files, missing files, read-only files, invalid UTF-8, and permission failures without terminating. Close programs that lock the file and retry. If the application folder is read-only, move the application to a writable folder.

Writers use an exclusive file handle and publish changes through a temporary file in the same directory. Successful animal changes retain the previous file as `animals.backup.txt`. A stale or ambiguous selection cannot overwrite or delete a newer record. Close external editors before making changes in the application.

To recover the previous animal file, close the application, copy `animals.backup.txt` over `animals.txt`, then reopen the application. The backup holds the version immediately before the last successful animal change. Summary output can always be regenerated.

## Build and checks

```powershell
dotnet build PRG272.sln --configuration Release
dotnet run --project tests/WildlifeRehabilitation.Checks --configuration Release --no-build
```

The checks use isolated temporary fixtures. They cover malformed records, invalid UTF-8, score boundaries including 39, shared validation, duplicates, Unicode, locked and read-only files, competing writers, atomic publication, backups, precise updates and deletions, stale selections, and valid-record summary calculations. GitHub Actions runs the Release build and checks on Windows for every pull request and main-branch push.

`Run.cmd --check` exercises the launcher's prerequisite and build steps without opening a window.

## Feature history

| Branch | Scope |
| --- | --- |
| `feature/view-animals` | Blank .NET 8 WinForms foundation, shared contract, read-only grid, starter data, file loading, skipped-line count, refresh |
| `feature/add-animal` | Editor, shared validation and classification, ID suggestions, safe appending, error feedback |
| `feature/update-animal` | Find by ID or row selection, edit mode, shared validation, precise selected-record replacement |
| `feature/delete-animal` | Confirmation, precise removal, backup, grid refresh without closing the app |
| `feature/summary-report` | Valid-record totals and averages, on-screen report, UTF-8 output, run instructions |

Each feature has its own pull request. The blank WinForms project was committed before feature implementation in commit `c0737cd`. The original .NET Framework projects remain in Git history. The active solution contains one application and its checks.

See `docs/implementation.md` for the shared decisions and the Microsoft DataGridView sample provenance. Its MIT code license is retained in `THIRD-PARTY-NOTICES.txt`.
