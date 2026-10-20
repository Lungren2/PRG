# Add animal: source and code map

Tian Heydenrych wrote the original Add project and owns Add animal and validation. The working app adapts his field handling, duplicate-ID scan, and five-case recovery classification switch. Integration added shared validation, a common file format, safer file writes, and the current UI. Those additions should not be attributed to Tian's original project.

His unchanged [original files](../reference/tian-add/README.md) are included for comparison and appear under **Tian Add source** in Visual Studio.

## Where his code went

| Original | Working app | What changed |
| --- | --- | --- |
| `Form1.Designer.cs`: `txtID`, `txtName`, `txtSpecies`, `txtAge`, `txtRecoveryScore` | [AnimalEditorForm.cs](../src/WildlifeRehabilitation/AnimalEditorForm.cs): the same five named fields | Controls use the current layout. Add and Update share the form. |
| `Form1.btnAdd_Click` | `AnimalEditorForm.btnSave_Click` | Reads those five fields. `TryCreate` reports invalid input instead of throwing from `int.Parse`. The callback saves before the dialog closes. |
| `Classes.TextWriter`: `foreach` ID scan and existence flag | [AnimalFileStore.cs](../src/WildlifeRehabilitation/Storage/AnimalFileStore.cs): `Append` | Retains the scan and flag. Compares text IDs, including IDs on malformed lines, before writing. |
| `Classes.TextWriter`: recovery-score `switch` | [AnimalRules.cs](../src/WildlifeRehabilitation/Domain/AnimalRules.cs): `Classify` | Keeps the five cases, `housingUnit` and `status` assignments. Includes 39 in Serious and fixes housing spelling. One shared switch replaces the two original copies. |
| `writer.WriteLine` in each case | `AnimalRules.Serialize`, then `AnimalFileStore.SaveAnimals` | One seven-field record format. Atomic replacement and a backup protect existing records if saving fails. |

## Follow one save

1. `MainForm.AddAnimal` opens the editor with a suggested ID and supplies `store.Append` as its save callback.
2. `btnSave_Click` reads the five text boxes into an `AnimalDraft`.
3. `AnimalRules.TryCreate` validates the draft and creates an `Animal`. Its `Status` and `HousingUnit` properties call Tian's adapted `Classify` switch.
4. `Append` locks the file and checks each raw line for a duplicate ID. It serializes the new animal and publishes the updated file with a backup.
5. Success closes the editor. `MainForm` reloads the grid and selects the added animal. Validation, duplicate, or file errors leave the fields available to correct and retry.

For example, score `39` follows the Serious case and produces `High-Dependency Ward`. After saving, `animals.txt` contains:

```text
WR-0011|Kaya|Serval|3|39|Serious|High-Dependency Ward
```

## Integration decisions to explain

- Numeric IDs and comma-separated records became `WR-0001` IDs and pipe-separated records so every feature reads the same data.
- `animals.txt` lives beside the executable. First launch creates starter records, rather than depending on the working directory.
- Successful saves close the editor and refresh the grid. The original stayed open and cleared its fields.
- Failed saves preserve input and report the error. The original could clear fields after a duplicate and always displayed `Done`.

Run `Run.cmd` to try this flow. Add an animal with score 39, then try its ID again to see the duplicate check without losing your entered fields. Cancel returns to the grid. If a file is locked, close the program holding it and retry Save.
