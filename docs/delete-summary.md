# Delete and summary: source and code map

Aseza Patiwe owns Delete animal and Summary report and wrote the original project. The working app adapts the selection and confirmation flow, list removal, report totals, averages, and five named status counters. Shared loading, stale-record checks, atomic writes, and the current UI are integration changes.

The unchanged [original source](../reference/delete-summary/README.md) appears under **Aseza Delete and Summary source** in Visual Studio.

## Where the code went

| Original | Working app | What changed |
| --- | --- | --- |
| `frmDelete.btnDelete_Click`: selected row, ID/name, Yes/No confirmation | [MainForm.cs](../src/WildlifeRehabilitation/MainForm.cs): `btnDelete_Click` | Same handler name and confirmation flow. Uses the row's stored record instead of reading cell text. No is the default. |
| `lines.RemoveAll`, then `File.WriteAllLines` | [AnimalFileStore.cs](../src/WildlifeRehabilitation/Storage/AnimalFileStore.cs): `Delete` | Retains list removal by selected ID. First locks the file and rejects stale or duplicate selections. Writes atomically, preserving other lines and a backup. |
| `frmSummaryReport.btnSummary_Click`: totals, loop, five counters, averages | [AnimalSummary.cs](../src/WildlifeRehabilitation/Domain/AnimalSummary.cs): `Create` | Retains the named counters and loop. Reads validated animals from the shared loader; totals use `long` to avoid integer overflow. |
| `rtbSummary.Text`, then saving `summary.txt` | [SummaryForm.cs](../src/WildlifeRehabilitation/SummaryForm.cs): `btnSummary_Click` and `rtbSummary` | Retains the handler/control names. Calculation and file writing have separate owners. If saving fails, the report stays visible with an error. |
| `btnGoToSummary_Click` and `btnBackToMain_Click` | MainMenuForm's Summary report action and SummaryForm's Close button | A modal report returns to the Main Menu without opening another Delete window. |

## Follow a delete

1. `MainForm.RefreshAnimals` loads the grid. The original `frmDelete_Load` was empty. Each selected row now holds a `StoredAnimal`, including the original file line.
2. `btnDelete_Click` checks the selection and asks to delete that ID and name. No leaves the record alone.
3. `AnimalFileStore.Delete` locks and rereads the file. `FindCurrentRecord` rejects a changed record, a missing ID, or an ID appearing more than once.
4. `RemoveAll` removes the unique selected ID. `SaveAnimals` publishes the remaining lines with a backup.
5. MainForm refreshes the grid. It stays open, unlike the original startup form's `Close` call.

## Follow a report

`SummaryForm.btnSummary_Click` loads valid unique records, calls `AnimalSummary.Create`, displays its text in `rtbSummary`, and saves the same text through `SaveSummary`.

The original `totalAnimals`, `totalAge`, `totalScore`, and five status counters remain recognizable. The loop counts only records accepted by the shared loader. This fixes the original denominator, which counted malformed nonblank lines even when their age and score did not contribute. Empty reports still show `N/A` averages and all five zero counts. Nonempty averages use invariant formatting with two decimal places.

Run `Run.cmd`, choose Remove an animal, select a row, and choose Delete selected. Check the ID/name, cancel once, then confirm a delete. Return to the menu and open Summary report to see the new totals and matching `summary.txt`. Locked-file or permission errors leave the data intact and show a message to correct before retrying.
