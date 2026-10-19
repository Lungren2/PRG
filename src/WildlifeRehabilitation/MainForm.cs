// Adapted from Microsoft's DataGridViewSimpleUnbound sample. See THIRD-PARTY-NOTICES.txt.
using WildlifeRehabilitation.Storage;
using WildlifeRehabilitation.Domain;

namespace WildlifeRehabilitation;

public sealed class MainForm : Form
{
    private readonly AnimalFileStore store = new(AppContext.BaseDirectory);
    private readonly DataGridView animalsGrid = new();
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private readonly Label status = new() { AutoSize = true, Padding = new Padding(0, 8, 0, 4) };
    private readonly Label fileLocation = new() { AutoSize = true, ForeColor = SystemColors.GrayText, Padding = new Padding(0, 0, 0, 4) };

    public MainForm()
    {
        Text = "Animal records | Baobab Ridge";
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1120, 640);
        MinimumSize = new Size(940, 500);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Animal records", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 6) }, 0, 0);
        layout.Controls.Add(new Label { Text = "Baobab Ridge Wildlife Rehabilitation Centre", AutoSize = true, Margin = new Padding(0, 0, 0, 14) }, 0, 1);
        layout.Controls.Add(actions, 0, 2);
        layout.Controls.Add(animalsGrid, 0, 3);
        layout.Controls.Add(status, 0, 4);
        layout.Controls.Add(fileLocation, 0, 5);
        Controls.Add(layout);
        AddAction("&Add animal", (_, _) => AddAnimal());
        AddAction("&Refresh", (_, _) => RefreshAnimals());
        SetupDataGridView();
        Shown += (_, _) => RefreshAnimals();
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
        var button = new Button { Text = text, AutoSize = true, Padding = new Padding(10, 5, 10, 5), Margin = new Padding(0, 0, 8, 12) };
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
        int[] widths = [95, 130, 170, 55, 60, 120, 190];
        for (var index = 0; index < headers.Length; index++)
            animalsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = headers[index], HeaderText = headers[index], MinimumWidth = widths[index], FillWeight = widths[index], SortMode = DataGridViewColumnSortMode.Automatic });
    }

    private bool RefreshAnimals(string? message = null, string? selectedId = null)
    {
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
            var prefix = document.Created ? "Created starter file. " : message is null ? "" : message + " ";
            status.Text = $"{prefix}{document.Records.Count} animals loaded. Skipped lines: {document.SkippedCount}." + (document.Records.Count == 0 ? " No animal records to display." : "");
            status.ForeColor = document.SkippedCount > 0 ? Color.Maroon : SystemColors.ControlText;
            fileLocation.Text = store.DataPath;
            return true;
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            animalsGrid.Rows.Clear();
            status.Text = (message is null ? "" : message + " ") + "Records could not be loaded. Resolve the file error and select Refresh.";
            status.ForeColor = Color.Maroon;
            fileLocation.Text = store.DataPath;
            UiErrors.Show(this, exception);
            return false;
        }
    }
}
