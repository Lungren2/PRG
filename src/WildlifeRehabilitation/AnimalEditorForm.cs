using System.Globalization;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

public sealed class AnimalEditorForm : WindowsBackdropForm
{
    private readonly Dictionary<string, TextBox> fields = new();
    // Tian's original Form1 field names, retained for tracing Add during presentations.
    private readonly TextBox txtID;
    private readonly TextBox txtName;
    private readonly TextBox txtSpecies;
    private readonly TextBox txtAge;
    private readonly TextBox txtRecoveryScore;
    private readonly ErrorProvider errors;
    private readonly Label classification = new() { AutoSize = true, MaximumSize = new Size(420, 0) };
    private readonly Label feedback = new() { AutoSize = true, ForeColor = Color.Maroon, MaximumSize = new Size(420, 0) };
    private readonly Action<Animal> save;
    public Animal? SavedAnimal { get; private set; }

    public AnimalEditorForm(string title, AnimalDraft initial, Action<Animal> save, bool lockId = false)
    {
        this.save = save;
        Text = title;
        DesktopTheme.Apply(this);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(590, 430);
        errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var layout = new TableLayoutPanel { BackColor = DesktopTheme.Canvas, Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 8 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var index = 0; index < 7; index++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        txtID = AddField(layout, "Animal &ID", nameof(initial.Id), initial.Id, 0);
        txtName = AddField(layout, "&Name", nameof(initial.Name), initial.Name, 1);
        txtSpecies = AddField(layout, "&Species", nameof(initial.Species), initial.Species, 2);
        txtAge = AddField(layout, "&Age in years", nameof(initial.Age), initial.Age, 3);
        txtRecoveryScore = AddField(layout, "&Recovery score (0-100)", nameof(initial.RecoveryScore), initial.RecoveryScore, 4);
        fields[nameof(initial.Id)].ReadOnly = lockId;
        fields[nameof(initial.Id)].TabStop = !lockId;
        if (lockId) fields[nameof(initial.Id)].BackColor = DesktopTheme.Canvas;
        layout.Controls.Add(new Label { Text = "Status / housing", AutoSize = true, ForeColor = DesktopTheme.Muted, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 12, 0) }, 0, 5);
        classification.BackColor = DesktopTheme.Surface;
        classification.ForeColor = DesktopTheme.Accent;
        classification.Font = DesktopTheme.StrongFont;
        classification.BorderStyle = BorderStyle.Fixed3D;
        classification.Padding = new Padding(8, 6, 8, 6);
        classification.MaximumSize = new Size(340, 0);
        layout.Controls.Add(classification, 1, 5);
        feedback.ForeColor = DesktopTheme.Error;
        feedback.MaximumSize = new Size(520, 0);
        feedback.Margin = new Padding(0, 8, 0, 4);
        layout.Controls.Add(feedback, 0, 6);
        layout.SetColumnSpan(feedback, 2);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new DesktopButton { Text = "Cancel", DialogResult = DialogResult.Cancel };
        var saveButton = new DesktopButton { Text = "&Save" };
        DesktopTheme.Style(cancel);
        DesktopTheme.Style(saveButton, primary: true);
        saveButton.Click += btnSave_Click;
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(saveButton);
        layout.Controls.Add(buttons, 0, 7);
        layout.SetColumnSpan(buttons, 2);
        AcceptButton = saveButton;
        CancelButton = cancel;
        Controls.Add(layout);
        fields[nameof(initial.RecoveryScore)].TextChanged += (_, _) => UpdateClassification();
        UpdateClassification();
        Shown += (_, _) => fields[lockId ? nameof(initial.Name) : nameof(initial.Id)].Focus();
    }

    private TextBox AddField(TableLayoutPanel layout, string label, string name, string initial, int row)
    {
        layout.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = DesktopTheme.Muted, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 16, 12) }, 0, row);
        var field = new TextBox { Name = name, AccessibleName = name, Text = initial, Dock = DockStyle.Top, Margin = new Padding(0, 0, 16, 12), MaxLength = name is "Name" or "Species" ? 80 : 20 };
        DesktopTheme.Style(field);
        fields.Add(name, field);
        layout.Controls.Add(field, 1, row);
        return field;
    }

    private void UpdateClassification()
    {
        if (int.TryParse(txtRecoveryScore.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var score) && score <= 100)
        {
            var result = AnimalRules.Classify(score);
            classification.Text = $"{result.Status} / {result.HousingUnit}";
        }
        else classification.Text = "Enter a valid score to preview status and housing.";
    }

    // Adapted from Tian's Form1.btnAdd_Click. Shared validation replaces int.Parse;
    // the save callback is Append for Add and Update for an existing record.
    private void btnSave_Click(object? sender, EventArgs e)
    {
        errors.Clear();
        feedback.Text = "";
        var draft = new AnimalDraft(txtID.Text, txtName.Text, txtSpecies.Text, txtAge.Text, txtRecoveryScore.Text);
        if (!AnimalRules.TryCreate(draft, out var animal, out var problems))
        {
            foreach (var problem in problems) errors.SetError(fields[problem.Key], problem.Value);
            feedback.Text = "Correct the marked fields. " + problems.Values.First();
            fields[problems.Keys.First()].Focus();
            return;
        }
        try
        {
            save(animal!);
            SavedAnimal = animal;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (AnimalConflictException exception) { feedback.Text = exception.Message; }
        catch (Exception exception) when (UiErrors.IsFileError(exception)) { feedback.Text = UiErrors.Describe(exception); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) errors.Dispose();
        base.Dispose(disposing);
    }
}
