// Adapted from Microsoft's DataGridViewSimpleUnbound sample. See THIRD-PARTY-NOTICES.txt.
// View/file loading and Main Menu navigation contribution: Nkosinathi Mathenjwa (604435).
using WildlifeRehabilitation.Storage;
using WildlifeRehabilitation.Domain;

namespace WildlifeRehabilitation;

public enum AnimalViewMode { View, Add, Update, Delete }

public sealed class MainForm : WindowsBackdropForm
{
    private readonly AnimalFileStore store;
    private readonly AnimalEditorPanel editor;
    private readonly Func<Animal, bool> confirmDeletion;
    private readonly DataGridView animalsGrid = new();
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private readonly Label status = new() { Name = "statusMessage", AutoSize = true, Padding = new Padding(0, 8, 0, 4) };
    private readonly Label fileLocation = new() { AutoSize = false, AutoEllipsis = true, Dock = DockStyle.Fill, Height = 24, Padding = new Padding(0, 0, 0, 4) };
    private readonly TextBox findId = new() { Name = "searchId", AccessibleName = "Search animal ID", PlaceholderText = "WR-0001", Width = 110, Margin = new Padding(0, 6, 8, 0), MaxLength = 20 };
    private StoredAnimal? SelectedRecord => animalsGrid.SelectedRows.Count == 1 ? animalsGrid.SelectedRows[0].Tag as StoredAnimal : null;

    // Opens the startup register, with optional dependencies for isolated UI checks.
    public MainForm(string? dataDirectory = null, Func<Animal, bool>? confirmDeletion = null)
        : this(new AnimalFileStore(dataDirectory ?? AppContext.BaseDirectory), AnimalViewMode.Update, confirmDeletion) { }

    // Builds Nkosinathi's shared grid/navigation with the editor needed by Add and Update.
    public MainForm(AnimalFileStore store, AnimalViewMode mode = AnimalViewMode.View, Func<Animal, bool>? confirmDeletion = null)
    {
        this.store = store;
        this.confirmDeletion = confirmDeletion ?? ConfirmDeletion;
        editor = new AnimalEditorPanel(store);
        editor.Visible = mode is AnimalViewMode.Add or AnimalViewMode.Update;
        Text = "Animal records | Baobab Ridge";
        DesktopTheme.Apply(this);
        ClientSize = new Size(1120, 650);
        MinimumSize = new Size(980, 550);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { BackColor = DesktopTheme.Canvas, Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        actions.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(actions, 0, 0);
        layout.Controls.Add(editor, 0, 1);
        var gridSurface = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.Fixed3D, BackColor = DesktopTheme.Surface, Margin = Padding.Empty };
        gridSurface.Controls.Add(animalsGrid);
        layout.Controls.Add(gridSurface, 0, 2);
        layout.Controls.Add(status, 0, 3);
        layout.Controls.Add(fileLocation, 0, 4);
        Controls.Add(layout);
        var backButton = AddAction("btnBack", "&Main Menu", (_, _) => OpenMainMenu());
        CancelButton = backButton;
        var deleteButton = AddAction("btnDelete", "&Delete selected", btnDelete_Click);
        deleteButton.Visible = mode != AnimalViewMode.View;
        DesktopTheme.Style(deleteButton, danger: true);
        AddAction("btnSummary", "&Generate Summary", (_, _) => { using var summary = new SummaryForm(store); summary.ShowDialog(this); });
        AddAction("btnRefresh", "&Refresh", (_, _) => RefreshAnimals());
        var searchControls = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        searchControls.Visible = mode is AnimalViewMode.Add or AnimalViewMode.Update;
        searchControls.Controls.Add(new Label { Text = "Animal ID", AutoSize = true, ForeColor = DesktopTheme.Muted, Margin = new Padding(8, 10, 8, 0) });
        DesktopTheme.Style(findId);
        findId.Margin = new Padding(0, 5, 8, 0);
        searchControls.Controls.Add(findId);
        var searchButton = new DesktopButton { Name = "btnSearch", Text = "&Search" };
        DesktopTheme.Style(searchButton);
        searchButton.Click += (_, _) => FindAnimal();
        searchControls.Controls.Add(searchButton);
        actions.Controls.Add(searchControls);
        findId.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FindAnimal(); } };
        SetupDataGridView();
        DesktopTheme.Style(animalsGrid);
        foreach (var name in new[] { "Age", "Recovery Score" })
        {
            animalsGrid.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            animalsGrid.Columns[name].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        editor.AnimalSaved += animal => RefreshAnimals(selectedId: animal.Id);
        layout.SizeChanged += (_, _) => status.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 36), 0);
        animalsGrid.CellClick += (_, e) => { if (editor.Visible && e.RowIndex >= 0 && SelectedRecord is { } selected) editor.LoadRecord(selected); };
        animalsGrid.KeyDown += (_, e) => { if (mode != AnimalViewMode.View && e.KeyCode == Keys.Delete) { e.SuppressKeyPress = true; btnDelete_Click(this, EventArgs.Empty); } };
        Shown += (_, _) => RefreshAnimals();
    }

    // Returns a feature dialog to Nkosinathi's menu, or opens that menu from the startup register.
    private void OpenMainMenu()
    {
        if (Modal) { Close(); return; }
        using var menu = new MainMenuForm(store);
        menu.ShowDialog(this);
        RefreshAnimals();
    }

    // Shows Aseza Patiwe's ID/name Yes/No confirmation, defaulting to No.
    private bool ConfirmDeletion(Animal animal) => MessageBox.Show(this,
        $"Delete {animal.Id} ({animal.Name})? This cannot be undone.", "Confirm delete",
        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    // Adapts Aseza's selection -> confirmation -> removal flow; the register stays open.
    private void btnDelete_Click(object? sender, EventArgs e)
    {
        var selected = SelectedRecord;
        if (selected is null)
        {
            status.Text = "Select an animal from the list to delete.";
            status.ForeColor = DesktopTheme.Error;
            return;
        }
        if (!confirmDeletion(selected.Animal)) return;
        try
        {
            store.Delete(selected);
            if (editor.EditingId == selected.Animal.Id) editor.ClearInputs();
            RefreshAnimals($"Deleted {selected.Animal.Id}.");
        }
        catch (AnimalConflictException exception) { status.Text = exception.Message; status.ForeColor = DesktopTheme.Error; }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { status.Text = UiErrors.Describe(exception); status.ForeColor = DesktopTheme.Error; }
    }

    // Finds an exact, case-insensitive ID and loads it into the same editor used by row clicks.
    private void FindAnimal()
    {
        var id = findId.Text.Trim().ToUpperInvariant();
        if (id.Length == 0) { status.Text = "Enter an Animal ID, then select Search."; findId.Focus(); return; }
        try
        {
            var document = store.Load();
            var found = document.Records.SingleOrDefault(record => record.Animal.Id == id);
            if (found is null) { status.Text = $"Animal ID {id} was not found."; status.ForeColor = DesktopTheme.Error; return; }
            PopulateGrid(document, id, $"Found {id}.");
            editor.LoadRecord(found);
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { status.Text = UiErrors.Describe(exception); status.ForeColor = DesktopTheme.Error; }
    }

    // Creates a consistently styled and named command-bar button.
    private Button AddAction(string name, string text, EventHandler onClick)
    {
        var button = new DesktopButton { Name = name, Text = text };
        DesktopTheme.Style(button);
        button.Click += onClick;
        actions.Controls.Add(button);
        return button;
    }

    // Configures the read-only, full-row grid with the brief's exact ordered column headers.
    private void SetupDataGridView()
    {
        animalsGrid.Name = "animalsGrid";
        animalsGrid.AccessibleName = "Animals";
        animalsGrid.Dock = DockStyle.Fill;
        animalsGrid.ReadOnly = true;
        animalsGrid.AllowUserToAddRows = false;
        animalsGrid.AllowUserToDeleteRows = false;
        animalsGrid.AllowUserToResizeRows = false;
        animalsGrid.RowHeadersVisible = false;
        animalsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        animalsGrid.MultiSelect = false;
        animalsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        animalsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        animalsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        animalsGrid.BackgroundColor = SystemColors.Window;
        animalsGrid.GridColor = SystemColors.ControlLight;
        animalsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        animalsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        string[] headers = ["Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit"];
        int[] widths = [100, 105, 140, 70, 145, 130, 185];
        for (var index = 0; index < headers.Length; index++)
            animalsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = headers[index], HeaderText = headers[index], MinimumWidth = widths[index], FillWeight = widths[index], SortMode = DataGridViewColumnSortMode.NotSortable });
    }

    // Reloads after startup or mutation and reports a file failure without closing the app.
    private void RefreshAnimals(string? message = null, string? selectedId = null)
    {
        try { PopulateGrid(store.Load(), selectedId ?? SelectedRecord?.Animal.Id, message); }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            animalsGrid.Rows.Clear();
            status.Text = "Records could not be loaded. " + UiErrors.Describe(exception);
            status.ForeColor = DesktopTheme.Error;
            fileLocation.Text = store.DataPath;
        }
    }

    // Replaces grid rows in file order and shows the skipped count once per load in the status line.
    private void PopulateGrid(AnimalDocument document, string? selectedId, string? message)
    {
        animalsGrid.Rows.Clear();
        foreach (var record in document.Records)
        {
            var animal = record.Animal;
            var index = animalsGrid.Rows.Add(animal.Id, animal.Name, animal.Species, animal.Age, animal.RecoveryScore, animal.Status, animal.HousingUnit);
            animalsGrid.Rows[index].Tag = record;
            if (animal.Id == selectedId) animalsGrid.CurrentCell = animalsGrid.Rows[index].Cells[0];
        }
        var prefix = document.Created ? "Created empty animals.txt. " : message is null ? "" : message + " ";
        status.Text = $"{prefix}{document.Records.Count} animals loaded. Skipped lines: {document.SkippedCount}.";
        status.ForeColor = document.SkippedCount > 0 ? DesktopTheme.Error : DesktopTheme.Muted;
        fileLocation.ForeColor = DesktopTheme.Muted;
        fileLocation.Text = store.DataPath;
    }
}
