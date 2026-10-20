# PRG272

A .NET 8 Windows Forms animal register for Baobab Ridge Wildlife Rehabilitation Centre.

## Run

On Windows, install the .NET 8 SDK and double-click **Run.cmd**. It builds and opens the app. Keep the project in a writable folder. Build errors appear in `build.log`.

You can also open `PRG272.sln` in Visual Studio or run:

```powershell
dotnet run --project src/WildlifeRehabilitation
```

## Use

The **Main Menu** opens first using the supplied interface. It loads the animal file and provides Add new animal, View all animals, Update an animal, Remove an animal, and Summary report buttons.

View all animals shows a read-only grid with the seven required columns in file order. Use Back to return to the menu. On the Update page, click a row or search for an exact ID to open the existing editor. On the Remove page, select a row and choose Delete selected. Deletion asks for confirmation. Summary report shows totals and averages and saves `summary.txt`.

The project includes `src/WildlifeRehabilitation/animals.txt` with ten starter records across all five statuses. **Copy if newer** places it beside the executable when building. If the running application finds no file, it creates an empty one. Existing files are never populated with extra sample records by the loader.

`animals.txt` is UTF-8 with one record per line: `ID|Name|Species|Age|Score|Status|Housing Unit`. Loading skips and counts malformed, duplicate, and blank lines. Changes refresh the grid and retain the previous file in `animals.backup.txt`. `summary.txt` is an overwritten UTF-8 report with a generation timestamp, total count, two averages, and all five status counts.

## Check

```powershell
dotnet build PRG272.sln -c Release
dotnet run --project tests/WildlifeRehabilitation.Checks -c Release --no-build
```

## Feature owners

| Feature | Owner | Student number |
| --- | --- | --- |
| [Add animal and validation](docs/add-animal.md) | Tian Heydenrych | 577481 |
| [View animals and file loading](docs/view-animals.md) | Nkosinathi Mathenjwa | 604435 |
| Update animal and integration | Ethan Ogle | 602114 |
| [Delete animal and summary report](docs/delete-summary.md) | Aseza Patiwe | 605131 |

The linked code maps show the original source, reused logic, and integration changes.

See [data rules and recovery](docs/implementation.md) for details. Each feature has its own branch and merged pull request.

## Known limitations

The desktop app requires Windows and the .NET 8 SDK to build. Keep it in a writable folder. Changes made in another text editor appear after Refresh or reopening a feature. Close programs that lock the file before saving. Update uses the existing shared modal editor; Back returns to the Main Menu.
