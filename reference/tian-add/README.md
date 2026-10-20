# Tian's original Add source

Tian wrote this Add project before consolidation. These three files are unchanged copies from `AddUserInterface (3)/AddUserInterface`, also imported in commit `a718eb51255279cbd4a8a6dc3ae318711014808c` under `src/AddUserInterface`.

- `Form1.Designer.cs` defines the five text boxes and Add/Back buttons.
- `Form1.cs` reads the fields in `btnAdd_Click` and calls `Classes.TextWriter`.
- `Classes.cs` checks duplicate IDs, classifies recovery scores, and writes the record.

These files are reference material, outside the compiled application. They retain the original format and bugs for an accurate comparison. Open the **Tian Add source** folder in `PRG272.sln` to find them in Visual Studio.

The working app adapts this code. See [the Add code map](../../docs/add-animal.md) for the current methods and the changes made during integration.
