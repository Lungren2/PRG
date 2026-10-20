# PRG272

A .NET 8 Windows Forms animal register for Baobab Ridge Wildlife Rehabilitation Centre.

## Run

On Windows, install the .NET 8 SDK and double-click **Run.cmd**. It builds and opens the app. Keep the project in a writable folder. Build errors appear in `build.log`.

You can also open `PRG272.sln` in Visual Studio or run:

```powershell
dotnet run --project src/WildlifeRehabilitation
```

## Use

Add animals, find them by ID, edit a selected row, or delete with confirmation. Summary report shows totals and averages and saves `summary.txt`.

The app creates ten starter records on first use. UTF-8 data stays in `animals.txt` beside the executable. Changes refresh the grid and keep a previous version in `animals.backup.txt`. Malformed records are skipped and counted.

## Check

```powershell
dotnet build PRG272.sln -c Release
dotnet run --project tests/WildlifeRehabilitation.Checks -c Release --no-build
```

## Feature owners

| Feature | Owner |
| --- | --- |
| [Add animal and validation](docs/add-animal.md) | Tian Heydenrych |
| View animals and file loading | Nkosinathi Mathenjwa |
| Update animal and integration | Ethan Ogle |
| [Delete animal and summary report](docs/delete-summary.md) | Aseza Patiwe |

The linked code maps show the original source, reused logic, and integration changes.

See [data rules and recovery](docs/implementation.md) for details. Each feature has its own branch and merged pull request.
