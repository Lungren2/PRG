// Adapted from Microsoft's DataGridViewSimpleUnbound sample. See THIRD-PARTY-NOTICES.txt.
using WildlifeRehabilitation.Storage;
using WildlifeRehabilitation.Domain;

namespace WildlifeRehabilitation;

public sealed class MainForm : WindowsBackdropForm
{
    private readonly AnimalFileStore store = new(AppContext.BaseDirectory);
    private readonly DataGridView animalsGrid = new();
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private readonly Label status = new() { AutoSize = true, Padding = new Padding(0, 8, 0, 4) };
    private readonly Label fileLocation = new() { AutoSize = false, AutoEllipsis = true, Dock = DockStyle.Fill, Height = 24, ForeColor = SystemColors.GrayText, Padding = new Padding(0, 0, 0, 4) };
    private readonly TextBox findId = new() { AccessibleName = "Find animal ID", PlaceholderText = "WR-0001", Width = 110, Margin = new Padding(0, 6, 8, 0), MaxLength = 20 };
    private readonly Button updateButton;
    private readonly Button deleteButton;
    private StoredAnimal? SelectedRecord => animalsGrid.CurrentRow?.Tag as StoredAnimal;

    public MainForm()
    {
        Text = "Animal records | Baobab Ridge";
        DesktopTheme.Apply(this);
        ClientSize = new Size(1120, 600);
        MinimumSize = new Size(980, 500);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { BackColor = DesktopTheme.Canvas, Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        actions.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(actions, 0, 0);
        var gridSurface = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.Fixed3D, BackColor = DesktopTheme.Surface, Margin = Padding.Empty };
        gridSurface.Controls.Add(animalsGrid);
        layout.Controls.Add(gridSurface, 0, 1);
        layout.Controls.Add(status, 0, 2);
        layout.Controls.Add(fileLocation, 0, 3);
        Controls.Add(layout);
        var addButton = AddAction("&Add animal", (_, _) => AddAnimal());
        DesktopTheme.Style(addButton, primary: true);
        updateButton = AddAction("&Edit selected", (_, _) => EditSelected());
        deleteButton = AddAction("&Delete selected", (_, _) => DeleteSelected());
        DesktopTheme.Style(deleteButton, danger: true);
        AddAction("&Summary report", (_, _) => { using var summary = new SummaryForm(store); summary.ShowDialog(this); });
        AddAction("&Refresh", (_, _) => RefreshAnimals());
        var findControls = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        findControls.Controls.Add(new Label { Text = "Find ID", AutoSize = true, ForeColor = DesktopTheme.Muted, Margin = new Padding(8, 10, 8, 0) });
        DesktopTheme.Style(findId);
        findId.Margin = new Padding(0, 5, 8, 0);
        fileLocation.ForeColor = DesktopTheme.Muted;
        findControls.Controls.Add(findId);
        var findButton = new DesktopButton { Text = "&Find" };
        DesktopTheme.Style(findButton);
        findButton.Click += (_, _) => FindAnimal();
        findControls.Controls.Add(findButton);
        actions.Controls.Add(findControls);
        findId.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FindAnimal(); } };
        SetupDataGridView();
        DesktopTheme.Style(animalsGrid);
        foreach (var name in new[] { "Age", "Score" })
        {
            animalsGrid.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            animalsGrid.Columns[name].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        updateButton.Enabled = deleteButton.Enabled = false;
        layout.SizeChanged += (_, _) => status.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 36), 0);
        animalsGrid.SelectionChanged += (_, _) => updateButton.Enabled = deleteButton.Enabled = SelectedRecord is not null;
        animalsGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };
        animalsGrid.KeyDown += (_, e) => { if (e.KeyCode == Keys.Delete) { e.SuppressKeyPress = true; DeleteSelected(); } };
        Shown += (_, _) => RefreshAnimals();
    }

    private void DeleteSelected()
    {
        var selected = SelectedRecord;
        if (selected is null) return;
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

    private void EditSelected()
    {
        var selected = SelectedRecord;
        if (selected is null) return;
        var animal = selected.Animal;
        using var editor = new AnimalEditorForm("Update " + animal.Id,
            new AnimalDraft(animal.Id, animal.Name, animal.Species, animal.Age.ToString(), animal.RecoveryScore.ToString()),
            replacement => store.Update(selected, replacement), lockId: true);
        if (editor.ShowDialog(this) == DialogResult.OK)
            RefreshAnimals($"Updated {animal.Id}.", animal.Id);
    }

    private void FindAnimal()
    {
        var id = findId.Text.Trim().ToUpperInvariant();
        if (id.Length == 0) { status.Text = "Enter an animal ID, such as WR-0001, then select Find."; findId.Focus(); return; }
        if (!RefreshAnimals(selectedId: id)) return;
        if (SelectedRecord?.Animal.Id == id) status.Text = $"Found {id}. " + status.Text;
        else { status.Text = $"No valid animal record has ID {id}. " + status.Text; findId.Focus(); }
    }

    private void AddAnimal()
    {
        try
        {
            using var editor = new AnimalEditorForm("Add animal", new AnimalDraft(store.SuggestId(), "", "", "", ""), store.Append);
            if (editor.ShowDialog(this) == DialogResult.OK)
                RefreshAnimals($"Added {editor.SavedAnimal!.Id}.", editor.SavedAnimal.Id);
        }
        catch (AnimalConflictException exception) { MessageBox.Show(this, exception.Message, "Cannot add animal", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { UiErrors.Show(this, exception); }
    }

    private Button AddAction(string text, EventHandler onClick)
    {
        var button = new DesktopButton { Text = text };
        DesktopTheme.Style(button);
        button.Click += onClick;
        actions.Controls.Add(button);
        return button;
    }

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
        string[] headers = ["Animal ID", "Name", "Species", "Age", "Score", "Status", "Housing unit"];
        int[] widths = [110, 130, 170, 65, 75, 130, 210];
        for (var index = 0; index < headers.Length; index++)
            animalsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = headers[index], HeaderText = headers[index], MinimumWidth = widths[index], FillWeight = widths[index], SortMode = DataGridViewColumnSortMode.Automatic });
    }

    private bool RefreshAnimals(string? message = null, string? selectedId = null)
    {
        selectedId ??= SelectedRecord?.Animal.Id;
        try
        {
            var document = store.Load();
            animalsGrid.Rows.Clear();
            foreach (var record in document.Records)
            {
                var animal = record.Animal;
                var index = animalsGrid.Rows.Add(animal.Id, animal.Name, animal.Species, animal.Age, animal.RecoveryScore, animal.Status, animal.HousingUnit);
                animalsGrid.Rows[index].Tag = record;
                if (animal.Id == selectedId) animalsGrid.CurrentCell = animalsGrid.Rows[index].Cells[0];
            }
            updateButton.Enabled = deleteButton.Enabled = SelectedRecord is not null;
            var prefix = document.Created ? "Created starter file. " : message is null ? "" : message + " ";
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
