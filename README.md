# PRG272

One .NET 8 Windows Forms application for the Baobab Ridge Wildlife Rehabilitation Centre.

## Start

On Windows, install the .NET 8 SDK or a newer SDK with .NET 8 targeting support. Open `PRG272.sln` in Visual Studio with the .NET desktop development workload, or run:

```powershell
dotnet run --project src/WildlifeRehabilitation
```

The application is being integrated through `feature/view-animals`, `feature/add-animal`, `feature/update-animal`, `feature/delete-animal`, and `feature/summary-report`. Each feature will have a merged pull request. The first commit on `feature/view-animals` is the blank WinForms foundation, before feature implementation.

The original .NET Framework projects remain in Git history. The active solution contains only the new application.
