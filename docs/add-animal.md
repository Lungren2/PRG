# Add animal: source and code map

Tian Heydenrych wrote the original Add project and owns Add animal and validation. The working app adapts his field handling, duplicate-ID scan, and five-case recovery classification switch. Integration added shared validation, a common file format, safer file writes, and the current UI. Those additions should not be attributed to Tian's original project.

His unchanged [original files](../reference/tian-add/README.md) are included for comparison and appear under **Tian Add source** in Visual Studio.

## Where his code went

| Original | Working app | What changed |
| --- | --- | --- |
| `Form1.Designer.cs`: `txtID`, `txtName`, `txtSpecies`, `txtAge`, `txtRecoveryScore` | [AnimalEditorPanel.cs](../src/WildlifeRehabilitation/AnimalEditorPanel.cs): the same five named fields | Controls use the current layout. Add and Update share the five inputs above the grid. |
| `Form1.btnAdd_Click` | `AnimalEditorPanel.btnSave_Click` | Reads those five fields. `TryCreate` reports invalid input instead of throwing from `int.Parse`. Successful saves clear the fields while the form stays open, as in Tian's original. |
| `Classes.TextWriter`: `foreach` ID scan and existence flag | [AnimalFileStore.cs](../src/WildlifeRehabilitation/Storage/AnimalFileStore.cs): `Append` | Retains the scan and flag. Compares text IDs, including IDs on malformed lines, before writing. |
| `Classes.TextWriter`: recovery-score `switch` | [AnimalRules.cs](../src/WildlifeRehabilitation/Domain/AnimalRules.cs): `Classify` | Keeps the five cases, `housingUnit` and `status` assignments. Includes 39 in Serious and fixes housing spelling. One shared switch replaces the two original copies. |
| `writer.WriteLine` in each case | `AnimalRules.Serialize`, then `AnimalFileStore.SaveAnimals` | One seven-field record format. Atomic replacement and a backup protect existing records if saving fails. |

## Follow one save

1. `MainForm` creates one `AnimalEditorPanel` with Add, Save Changes, and Clear buttons.
2. `btnSave_Click` reads the five text boxes into an `AnimalDraft`.
3. `AnimalRules.TryCreate` validates the draft and creates an `Animal`. Its `Status` and `HousingUnit` properties call Tian's adapted `Classify` switch.
4. `Append` locks the file and checks each raw line for a duplicate ID. It serializes the new animal and publishes the updated file with a backup.
5. Success clears the inputs, shows the saved status/housing, and raises `AnimalSaved` so MainForm reloads the grid. Validation, duplicate, or file errors preserve the entered fields.

Clicking a grid row or using Search calls the same `LoadRecord` method. It makes the ID read-only, disables Add, and enables Save Changes. Saving calls `store.Update` with the loaded record; Clear leaves edit mode without writing.

For example, score `39` follows the Serious case and produces `High-Dependency Ward`. After saving, `animals.txt` contains:

```text
WR-0011|Kaya|Serval|3|39|Serious|High-Dependency Ward
```

## Integration decisions to explain

- Numeric IDs and comma-separated records became `WR-` plus four digits and pipe-separated records, as required by the brief. `WR-0000` is valid too.
- Names/species are 1-30 trimmed characters. Age and score are whole numbers from 0 to 100.
- Builds copy the committed starter beside the executable if newer. A missing runtime file is created empty.
- Successful saves keep the shared form open, clear the fields, and refresh the grid.
- Failed saves preserve input and report the error. The original could clear fields after a duplicate and always displayed `Done`.

Run `Run.cmd` to try this flow. Add an animal with score 39, then try its ID again to see the duplicate check without losing your entered fields. Use Clear to leave edit mode. If a file is locked, close the program holding it and retry Add or Save Changes.
