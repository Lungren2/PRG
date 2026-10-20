# View animals and file loading

Owner: Nkosinathi Mathenjwa, 604435.

This contribution adds the supplied Main Menu, connects it to the existing features, and completes the file-loading and read-only-grid behaviour. Existing `src`, `Domain`, `Storage`, `Data`, `tests`, `tools`, and `reference` folders remain in place.

## Code map

| File | Responsibility |
| --- | --- |
| `src/WildlifeRehabilitation/Program.cs` | Starts the shared register with its grid loaded, as required by the brief. Main Menu remains accessible from that register. |
| `src/WildlifeRehabilitation/MainMenuForm.cs` | Displays the five menu buttons, loads the file at startup, and opens the existing feature forms. Uses the koala from the supplied menu image. |
| `src/WildlifeRehabilitation/MainForm.cs` | Builds the seven read-only grid columns, selects whole rows, loads a fresh snapshot, displays skipped-line counts, and provides Main Menu and Refresh. Reuses the existing Update and Delete handlers on their menu pages. |
| `src/WildlifeRehabilitation/Storage/AnimalFileStore.cs` | Creates an empty missing file, reads valid animals in file order, and counts rejected lines. Keeps raw lines available for teammates' existing save methods. |
| `src/WildlifeRehabilitation/Domain/AnimalRules.cs` | Existing shared parser and validation. The small corrections allow any four-digit ID, limit Name and Species to 30 characters, and limit Age to 0–100, matching the brief. Classification stays shared. |
| `src/WildlifeRehabilitation/animals.txt` | Ten valid starter records, copied beside the executable with Copy if newer. Runtime creation of a missing file remains empty. |
| `tests/WildlifeRehabilitation.Checks/Program.cs` | Checks missing and empty files, malformed input, skipped counts, UTF-8, record order, repeated loads, file errors, and existing write behaviour. |
| `tools/Preview/Program.cs` | Checks actual menu navigation and grid controls, refresh after changes, Back navigation, and the shared Add/Update editor and Summary forms on Windows. |

## Follow one load

1. The startup register loads the grid in its `Shown` event. Opening Main Menu calls `LoadAnimals` and retains the supplied navigation and artwork.
2. `AnimalFileStore.Load` uses `animals.txt` in `AppContext.BaseDirectory`. When the file is missing, it creates an empty UTF-8 file.
3. `ReadDocument` loops through the file lines. The shared `TryParse` method splits each line on `|`, checks the seven fields, and uses `TryParse` for numbers.
4. Valid records enter a `List<StoredAnimal>` in the same order as the file. Invalid and repeated records increment the skipped count and do not enter the list.
5. Opening View all animals calls `RefreshAnimals`. It clears old rows, loops through the new list, and adds each record once. A status label reports the loaded and skipped counts.
6. Existing Update and Delete save handlers call the same refresh method after success. Adding from the shared editor refreshes its grid; opening View from the menu reads the same file. Repeated refreshes replace the old rows, so they cannot duplicate them.

The code uses classes, text-file handling, collections, loops, `TryParse`, error handling, and `DataGridView`. The shared file store continues to protect writes with its existing lock, temporary file, backup, and conflict checks. Tian's classification and Add logic, Ethan's Update integration, and Aseza's Delete and Summary logic are reused. Original reference source is unchanged.

## Manual demonstration

1. Run `Run.cmd`: the register displays the grid and loaded/skipped counts. Choose Main Menu to open the supplied menu.
2. Open View all animals: check the seven column headings, select a whole row, and verify that cells cannot be edited and no blank entry row appears.
3. Press Refresh repeatedly: records stay in file order without duplicates. Press Main Menu: the existing menu remains open.
4. Add an animal from the menu and reopen View. Update it from the Update page and verify its new score and classification. Delete it from the Remove page and verify that only its row disappears.
5. Close any text editor holding the file. Add a malformed line to the output `animals.txt`, then Refresh: that line is absent and the skipped count increases once.
6. Remove the output `animals.txt` while the app is open, then Refresh: an empty file and empty grid appear. Rebuilding may copy the committed starter file again if it is newer, as required by Copy if newer.

Use temporary copies when testing locked or read-only files. The existing friendly file-error handling keeps the application running and preserves data after a failed save.
