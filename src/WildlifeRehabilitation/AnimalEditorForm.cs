using System.Globalization;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

public sealed class AnimalEditorForm : Form
{
    private readonly Dictionary<string, TextBox> fields = new();
    private readonly ErrorProvider errors;
    private readonly Label classification = new() { AutoSize = true, MaximumSize = new Size(420, 0) };
    private readonly Label feedback = new() { AutoSize = true, ForeColor = Color.Maroon, MaximumSize = new Size(420, 0) };
    private readonly Action<Animal> save;
    public Animal? SavedAnimal { get; private set; }

    public AnimalEditorForm(string title, AnimalDraft initial, Action<Animal> save, bool lockId = false)
    {
        this.save = save;
        Text = title;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(570, 510);
        errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 2, RowCount = 9 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var index = 0; index < 8; index++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        AddField(layout, "Animal &ID", nameof(initial.Id), initial.Id, 0);
        AddField(layout, "&Name", nameof(initial.Name), initial.Name, 1);
        AddField(layout, "&Species", nameof(initial.Species), initial.Species, 2);
        AddField(layout, "&Age in years", nameof(initial.Age), initial.Age, 3);
        AddField(layout, "&Recovery score", nameof(initial.RecoveryScore), initial.RecoveryScore, 4);
        fields[nameof(initial.Id)].ReadOnly = lockId;
        fields[nameof(initial.Id)].TabStop = !lockId;
        layout.Controls.Add(new Label { Text = "Score must be from 0 to 100. Status and housing are assigned automatically.", AutoSize = true, MaximumSize = new Size(420, 0), Margin = new Padding(0, 0, 0, 12) }, 0, 5);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 5)!, 2);
        layout.Controls.Add(classification, 0, 6);
        layout.SetColumnSpan(classification, 2);
        feedback.Margin = new Padding(0, 12, 0, 8);
        layout.Controls.Add(feedback, 0, 7);
        layout.SetColumnSpan(feedback, 2);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        var saveButton = new Button { Text = "&Save", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        saveButton.Click += (_, _) => SaveAnimal();
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(saveButton);
        layout.Controls.Add(buttons, 0, 8);
        layout.SetColumnSpan(buttons, 2);
        AcceptButton = saveButton;
        CancelButton = cancel;
        Controls.Add(layout);
        fields[nameof(initial.RecoveryScore)].TextChanged += (_, _) => UpdateClassification();
        UpdateClassification();
        Shown += (_, _) => fields[lockId ? nameof(initial.Name) : nameof(initial.Id)].Focus();
    }

    private void AddField(TableLayoutPanel layout, string label, string name, string initial, int row)
    {
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 14, 14) }, 0, row);
        var field = new TextBox { Name = name, AccessibleName = name, Text = initial, Dock = DockStyle.Top, Margin = new Padding(0, 0, 20, 14), MaxLength = name is "Name" or "Species" ? 80 : 20 };
        fields.Add(name, field);
        layout.Controls.Add(field, 1, row);
    }

    private void UpdateClassification()
    {
        if (int.TryParse(fields[nameof(AnimalDraft.RecoveryScore)].Text, NumberStyles.None, CultureInfo.InvariantCulture, out var score) && score <= 100)
        {
            var result = AnimalRules.Classify(score);
            classification.Text = $"{result.Status} / {result.HousingUnit}";
        }
        else classification.Text = "Enter a valid score to preview status and housing.";
    }

    private void SaveAnimal()
    {
        errors.Clear();
        feedback.Text = "";
        var draft = new AnimalDraft(fields["Id"].Text, fields["Name"].Text, fields["Species"].Text, fields["Age"].Text, fields["RecoveryScore"].Text);
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
