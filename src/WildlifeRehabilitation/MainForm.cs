// View and navigation integration: Nkosinathi Mathenjwa (604435).
// Existing shared grid foundation: see THIRD-PARTY-NOTICES.txt.
using WildlifeRehabilitation.Storage;
using WildlifeRehabilitation.Domain;

namespace WildlifeRehabilitation;

public enum AnimalViewMode { View, Update, Delete }

public sealed class MainForm : Form
{
    private readonly AnimalFileStore store;
    private readonly DataGridView animalsGrid = new();
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private readonly Label status = new() { AutoSize = true, Padding = new Padding(0, 8, 0, 4) };
    private readonly Label fileLocation = new() { AutoSize = false, AutoEllipsis = true, Dock = DockStyle.Fill, Height = 24, ForeColor = SystemColors.GrayText, Padding = new Padding(0, 0, 0, 4) };
    private readonly TextBox findId = new() { AccessibleName = "Find animal ID", PlaceholderText = "WR-0001", Width = 110, Margin = new Padding(0, 6, 8, 0), MaxLength = 20 };
    private readonly Button updateButton;
    private readonly Button deleteButton;
    private StoredAnimal? SelectedRecord => animalsGrid.CurrentRow?.Tag as StoredAnimal;

    // Opens the view against animals.txt beside the executable.
    public MainForm() : this(new AnimalFileStore(AppContext.BaseDirectory)) { }

    // Builds the supplied grid-and-Back layout, reusing the existing Update and Delete handlers.
    public MainForm(AnimalFileStore store, AnimalViewMode mode = AnimalViewMode.View)
    {
        this.store = store;
        Text = mode == AnimalViewMode.Update ? "Update an animal" :
            mode == AnimalViewMode.Delete ? "Remove an animal" : "View all animals";
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Arial", 10F);
        BackColor = Color.White;
        ForeColor = Color.Black;
        ClientSize = new Size(1040, 480);
        MinimumSize = new Size(980, 400);
        StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { BackColor = BackColor, Dock = DockStyle.Fill, Padding = new Padding(30), ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(animalsGrid, 0, 0);
        actions.Margin = new Padding(0, 16, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        layout.Controls.Add(status, 0, 2);
        layout.Controls.Add(fileLocation, 0, 3);
        Controls.Add(layout);
        var backButton = AddAction("&Back", (_, _) => Close());
        backButton.Name = "btnBack";
        CancelButton = backButton;
        AddAction("&Refresh", (_, _) => RefreshAnimals()).Name = "btnRefresh";
        updateButton = AddAction("&Edit selected", (_, _) => EditSelected());
        updateButton.Visible = mode == AnimalViewMode.Update;
        deleteButton = AddAction("&Delete selected", btnDelete_Click);
        deleteButton.Visible = mode == AnimalViewMode.Delete;
        var findControls = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        findControls.Controls.Add(new Label { Text = "Find ID", AutoSize = true, ForeColor = DesktopTheme.Muted, Margin = new Padding(8, 10, 8, 0) });
        findId.BorderStyle = BorderStyle.FixedSingle;
        findId.Margin = new Padding(0, 5, 8, 0);
        fileLocation.ForeColor = DesktopTheme.Muted;
        findControls.Controls.Add(findId);
        var findButton = new Button { Text = "&Search", AutoSize = true, MinimumSize = new Size(80, 32) };
        findButton.Click += (_, _) => FindAnimal();
        findControls.Controls.Add(findButton);
        findControls.Visible = mode == AnimalViewMode.Update;
        actions.Controls.Add(findControls);
        findId.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FindAnimal(); } };
        SetupDataGridView();
        foreach (var name in new[] { "Age", "Recovery Score" })
        {
            animalsGrid.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            animalsGrid.Columns[name].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        updateButton.Enabled = deleteButton.Enabled = false;
        layout.SizeChanged += (_, _) => status.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 36), 0);
        animalsGrid.SelectionChanged += (_, _) => updateButton.Enabled = deleteButton.Enabled = SelectedRecord is not null;
        animalsGrid.CellClick += (_, e) => { if (mode == AnimalViewMode.Update && e.RowIndex >= 0) EditSelected(); };
        animalsGrid.KeyDown += (_, e) => { if (mode == AnimalViewMode.Delete && e.KeyCode == Keys.Delete) { e.SuppressKeyPress = true; btnDelete_Click(this, EventArgs.Empty); } };
        Shown += (_, _) => RefreshAnimals();
    }

    // Aseza Patiwe's frmDelete.btnDelete_Click.
    // Keep selection -> confirmation -> removal; refresh instead of closing the startup form.
    private void btnDelete_Click(object? sender, EventArgs e)
    {
        var selected = SelectedRecord;
        if (selected is null)
        {
            MessageBox.Show(this, "Select an animal row first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var result = MessageBox.Show(this,
            $"Delete {selected.Animal.Id} ({selected.Animal.Name})? This cannot be undone in the app.",
            "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes) return;
        try
        {
            store.Delete(selected);
            RefreshAnimals($"Deleted {selected.Animal.Id}.");
        }
        catch (AnimalConflictException exception) { MessageBox.Show(this, exception.Message, "Cannot delete animal", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { UiErrors.Show(this, exception); }
    }

    // Passes the selected record to existing shared editor and refreshes after saving.
    private void EditSelected()
    {
        var selected = SelectedRecord;
        if (selected is null)
        {
            MessageBox.Show(this, "Select an animal row first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var animal = selected.Animal;
        using var editor = new AnimalEditorForm("Update " + animal.Id,
            new AnimalDraft(animal.Id, animal.Name, animal.Species, animal.Age.ToString(), animal.RecoveryScore.ToString()),
            replacement => store.Update(selected, replacement), lockId: true);
        if (editor.ShowDialog(this) == DialogResult.OK)
            RefreshAnimals($"Updated {animal.Id}.", animal.Id);
    }

    // Finds an exact, case-insensitive ID and opens that record in the shared editor.
    private void FindAnimal()
    {
        var id = findId.Text.Trim().ToUpperInvariant();
        if (id.Length == 0) { status.Text = "Enter an animal ID, such as WR-0001, then select Find."; findId.Focus(); return; }
        if (!RefreshAnimals(selectedId: id)) return;
        if (SelectedRecord?.Animal.Id == id) EditSelected();
        else { status.Text = $"No valid animal record has ID {id}. " + status.Text; findId.Focus(); }
    }

    // Adds a labelled standard Windows button to the navigation row.
    private Button AddAction(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(100, 32),
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(0, 0, 12, 0) };
        button.FlatAppearance.BorderColor = Color.Black;
        button.Click += onClick;
        actions.Controls.Add(button);
        return button;
    }

    // Makes the seven required columns read-only and keeps records in their original file order.
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
        animalsGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        animalsGrid.BackgroundColor = SystemColors.Window;
        animalsGrid.GridColor = Color.Black;
        animalsGrid.BorderStyle = BorderStyle.FixedSingle;
        animalsGrid.EnableHeadersVisualStyles = false;
        animalsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        animalsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        animalsGrid.RowTemplate.Height = 28;
        animalsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        animalsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        string[] headers = ["Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit"];
        int[] widths = [100, 120, 145, 55, 130, 130, 200];
        for (var index = 0; index < headers.Length; index++)
            animalsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = headers[index], HeaderText = headers[index], MinimumWidth = widths[index], FillWeight = widths[index], SortMode = DataGridViewColumnSortMode.NotSortable });
    }

    // Replaces grid rows with one fresh file snapshot, preserving a selected ID when it still exists.
    private bool RefreshAnimals(string? message = null, string? selectedId = null)
    {
        selectedId ??= SelectedRecord?.Animal.Id;
        try
        {
            var document = store.Load();
            animalsGrid.Rows.Clear();
            DataGridViewRow? selectedRow = null;
            foreach (var record in document.Records)
            {
                var animal = record.Animal;
                var index = animalsGrid.Rows.Add(animal.Id, animal.Name, animal.Species, animal.Age, animal.RecoveryScore, animal.Status, animal.HousingUnit);
                animalsGrid.Rows[index].Tag = record;
                if (animal.Id == selectedId) selectedRow = animalsGrid.Rows[index];
            }
            // Adding rows can select the first row automatically; only restore an explicit selection.
            animalsGrid.ClearSelection();
            animalsGrid.CurrentCell = null;
            if (selectedRow != null) animalsGrid.CurrentCell = selectedRow.Cells[0];
            updateButton.Enabled = deleteButton.Enabled = SelectedRecord is not null;
            var prefix = document.Created ? "Created empty animals.txt. " : message is null ? "" : message + " ";
            status.Text = $"{prefix}{document.Records.Count} animals loaded. Skipped lines: {document.SkippedCount}." + (document.Records.Count == 0 ? " No animal records to display." : "");
            status.ForeColor = document.SkippedCount > 0 ? DesktopTheme.Error : DesktopTheme.Muted;
            fileLocation.Text = store.DataPath;
            return true;
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            animalsGrid.Rows.Clear();
            updateButton.Enabled = deleteButton.Enabled = false;
            status.Text = (message is null ? "" : message + " ") + "Records could not be loaded. Resolve the file error and select Refresh.";
            status.ForeColor = DesktopTheme.Error;
            fileLocation.Text = store.DataPath;
            UiErrors.Show(this, exception);
            return false;
        }
    }
}
