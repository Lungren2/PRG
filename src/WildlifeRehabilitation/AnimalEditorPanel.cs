using System.Globalization;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

public sealed class AnimalEditorPanel : UserControl
{
    private readonly AnimalFileStore store;
    private readonly Dictionary<string, TextBox> fields = new();
    // Tian Heydenrych's five original field names remain in the live Add/Update code.
    private readonly TextBox txtID;
    private readonly TextBox txtName;
    private readonly TextBox txtSpecies;
    private readonly TextBox txtAge;
    private readonly TextBox txtRecoveryScore;
    private readonly ErrorProvider errors;
    private readonly Label classification = new() { AutoSize = true, MinimumSize = new Size(300, 34), Visible = false };
    private readonly Label feedback = new() { Name = "editorFeedback", AutoSize = true };
    private readonly Button btnAdd = new DesktopButton { Name = "btnAdd", Text = "&Add" };
    private readonly Button btnSaveChanges = new DesktopButton { Name = "btnSaveChanges", Text = "&Save Changes", Enabled = false };
    private StoredAnimal? editingRecord;
    public string? EditingId => editingRecord?.Animal.Id;
    public event Action<Animal>? AnimalSaved;

    // Builds the five shared inputs and the explicit Add, Save Changes, and Clear modes.
    public AnimalEditorPanel(AnimalFileStore store)
    {
        this.store = store;
        Name = "animalEditor";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Fill;
        BackColor = DesktopTheme.Canvas;
        Font = DesktopTheme.BodyFont;
        errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var layout = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 5, RowCount = 4, Margin = Padding.Empty };
        foreach (var width in new[] { 15F, 25F, 25F, 15F, 20F })
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
        for (var row = 0; row < 4; row++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        txtID = AddField(layout, "Animal &ID", "Id", 0);
        txtName = AddField(layout, "&Name", "Name", 1);
        txtSpecies = AddField(layout, "&Species", "Species", 2);
        txtAge = AddField(layout, "&Age (years)", "Age", 3);
        txtRecoveryScore = AddField(layout, "&Recovery Score", "RecoveryScore", 4);
        classification.BackColor = DesktopTheme.Surface;
        classification.ForeColor = DesktopTheme.Accent;
        classification.Font = DesktopTheme.StrongFont;
        classification.BorderStyle = BorderStyle.Fixed3D;
        classification.Padding = new Padding(8, 6, 8, 6);
        layout.Controls.Add(classification, 0, 2);
        layout.SetColumnSpan(classification, 3);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        var btnClear = new DesktopButton { Name = "btnClear", Text = "&Clear" };
        DesktopTheme.Style(btnAdd, primary: true);
        DesktopTheme.Style(btnSaveChanges);
        DesktopTheme.Style(btnClear);
        btnAdd.Click += btnSave_Click;
        btnSaveChanges.Click += btnSave_Click;
        btnClear.Click += (_, _) => ClearInputs();
        buttons.Controls.Add(btnClear);
        buttons.Controls.Add(btnSaveChanges);
        buttons.Controls.Add(btnAdd);
        layout.Controls.Add(buttons, 3, 2);
        layout.SetColumnSpan(buttons, 2);
        feedback.ForeColor = DesktopTheme.Error;
        feedback.Margin = new Padding(0, 4, 0, 8);
        layout.Controls.Add(feedback, 0, 3);
        layout.SetColumnSpan(feedback, 5);
        layout.SizeChanged += (_, _) =>
        {
            classification.MaximumSize = new Size(Math.Max(300, layout.ClientSize.Width * 65 / 100 - 16), 0);
            feedback.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 16), 0);
        };
        Controls.Add(layout);
        txtRecoveryScore.TextChanged += (_, _) => UpdateClassification();
    }

    // Creates a labelled input while retaining the original five field identities.
    private TextBox AddField(TableLayoutPanel layout, string label, string name, int column)
    {
        layout.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = DesktopTheme.Muted, Margin = new Padding(0, 0, 8, 4) }, column, 0);
        var field = new TextBox { Name = name, AccessibleName = label.Replace("&", ""), Dock = DockStyle.Top, Margin = new Padding(0, 0, 18, 10) };
        DesktopTheme.Style(field);
        field.KeyDown += Field_KeyDown;
        fields.Add(name, field);
        layout.Controls.Add(field, column, 1);
        return field;
    }

    // Makes Enter in any animal field perform the currently enabled save action.
    private void Field_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        (editingRecord is null ? btnAdd : btnSaveChanges).PerformClick();
    }

    // Loads either a grid or search result into the same inputs with an immutable ID.
    public void LoadRecord(StoredAnimal record)
    {
        ClearInputs();
        editingRecord = record;
        txtID.Text = record.Animal.Id;
        txtName.Text = record.Animal.Name;
        txtSpecies.Text = record.Animal.Species;
        txtAge.Text = record.Animal.Age.ToString(CultureInfo.InvariantCulture);
        txtRecoveryScore.Text = record.Animal.RecoveryScore.ToString(CultureInfo.InvariantCulture);
        txtID.ReadOnly = true;
        txtID.TabStop = false;
        txtID.BackColor = DesktopTheme.Canvas;
        btnAdd.Enabled = false;
        btnSaveChanges.Enabled = true;
        feedback.Text = "Editing " + record.Animal.Id + ".";
        feedback.ForeColor = DesktopTheme.Muted;
        txtName.Focus();
    }

    // Leaves edit mode and clears all five inputs so another animal can be added immediately.
    public void ClearInputs()
    {
        editingRecord = null;
        foreach (var field in fields.Values) field.Clear();
        txtID.ReadOnly = false;
        txtID.TabStop = true;
        txtID.BackColor = DesktopTheme.Surface;
        btnAdd.Enabled = true;
        btnSaveChanges.Enabled = false;
        errors.Clear();
        classification.Text = "";
        classification.Visible = false;
        feedback.Text = "";
        txtID.Focus();
    }

    // Previews the single shared classification without letting the user edit its results.
    private void UpdateClassification()
    {
        if (int.TryParse(txtRecoveryScore.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var score) && score <= 100)
        {
            var result = AnimalRules.Classify(score);
            classification.Text = result.Status + " / " + result.HousingUnit;
            classification.Visible = true;
        }
        else { classification.Text = ""; classification.Visible = false; }
    }

    // Adapts Tian's btnAdd_Click: validate the five fields, save, then clear only on success.
    private void btnSave_Click(object? sender, EventArgs e)
    {
        errors.Clear();
        feedback.Text = "";
        feedback.ForeColor = DesktopTheme.Error;
        var draft = new AnimalDraft(txtID.Text, txtName.Text, txtSpecies.Text, txtAge.Text, txtRecoveryScore.Text);
        if (!AnimalRules.TryCreate(draft, out var animal, out var problems))
        {
            foreach (var problem in problems) errors.SetError(fields[problem.Key], problem.Value);
            feedback.Text = problems.Values.First();
            fields[problems.Keys.First()].Focus();
            return;
        }
        try
        {
            var operation = editingRecord is null ? "Added" : "Updated";
            if (editingRecord is null) store.Append(animal!);
            else store.Update(editingRecord, animal!);
            ClearInputs();
            feedback.ForeColor = DesktopTheme.Accent;
            feedback.Text = $"{operation} {animal!.Id}. {animal.Status} / {animal.HousingUnit}.";
            AnimalSaved?.Invoke(animal);
        }
        catch (AnimalConflictException exception)
        {
            feedback.Text = exception.Message;
            if (editingRecord is null) { errors.SetError(txtID, exception.Message); txtID.Focus(); }
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { feedback.Text = UiErrors.Describe(exception); }
    }

    // Releases the ErrorProvider owned by this reusable editor.
    protected override void Dispose(bool disposing)
    {
        if (disposing) errors.Dispose();
        base.Dispose(disposing);
    }
}
