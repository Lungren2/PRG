# PRG2782: Baobab Ridge Wildlife Rehabilitation Records System

A .NET 8 Windows Forms animal register for Baobab Ridge Wildlife Rehabilitation Centre.

## Run

On Windows, install the .NET 8 SDK and double-click **Run.cmd**. It builds and opens the app. Keep the project in a writable folder. Build errors appear in `build.log`.

You can also open `PRG272.sln` in Visual Studio or run:

```powershell
dotnet run --project src/WildlifeRehabilitation
```

## Use

The register opens with starter data loaded. Add animals, click a row or Search by ID to update, Clear to leave edit mode, and Delete with confirmation. Generate Summary shows totals, averages, and all five status counts. Main Menu opens the five feature pages; a page's Main Menu button returns to that menu.

Both files are UTF-8 beside the executable. `animals.txt` has seven pipe-separated fields: `ID|Name|Species|Age|Score|Status|Housing Unit`. The committed ten-record starter is copied if newer during builds. A missing runtime file is created empty. `summary.txt` is overwritten with the specified dated totals, two-decimal averages, and five status counts.

## Check

```powershell
dotnet build PRG272.sln -c Release
dotnet run --project tests/WildlifeRehabilitation.Checks -c Release --no-build
```

## Feature owners

| Feature | Owner | Student number |
| --- | --- | --- |
| Add and validation | Tian Heydenrych | 577481 |
| View and file loading | Nkosinathi Mathenjwa | 604435 |
| Update and integration | Ethan Ogle | 602114 |
| Delete and Summary | Aseza Patiwe | 605131 |

## Limitations and recovery

Windows only; run from a writable folder. Close programs locking a file and retry. Invalid lines are skipped and counted; conflicting IDs need repair in a text editor. To recover data, close the app and copy `animals.backup.txt` over `animals.txt`. Back up live data before replacing it with the starter.
