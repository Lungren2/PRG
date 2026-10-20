using System.Drawing.Imaging;
using System.Reflection;
using WildlifeRehabilitation;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

internal static class Program
{
    private static int checks;

    // Exercises the actual WinForms event handlers with isolated data and no desktop input.
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Pass an absolute output directory for the preview PNGs.");
        var output = Path.GetFullPath(args[0]);
        Directory.CreateDirectory(output);
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (var probe = new BackdropProbe())
        {
            _ = probe.Handle;
            Console.WriteLine("Windows acrylic API accepted: " + probe.NativeBackdropActive);
            using var nativePaint = new Bitmap(probe.Width, probe.Height);
            probe.DrawToBitmap(nativePaint, new Rectangle(Point.Empty, nativePaint.Size));
        }
        var fixtures = Path.Combine(Path.GetTempPath(), "PRG272-preview-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtures);
        var store = new AnimalFileStore(fixtures);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "animals.txt"), store.DataPath);
        var original = File.ReadAllText(store.DataPath);
        var allowDelete = false;
        Animal? confirmed = null;
        Render(new MainForm(fixtures, animal => { confirmed = animal; return allowDelete; }), new Size(1120, 650), "animal-records.png", output, form =>
        {
            var inputs = Descendants(form).OfType<TextBox>().ToDictionary(field => field.Name);
            var grid = Descendants(form).OfType<DataGridView>().Single();
            var editor = Descendants(form).OfType<AnimalEditorPanel>().Single();
            var expectedHeaders = new[] { "Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit" };
            Require(grid.Columns.Cast<DataGridViewColumn>().Select(column => column.HeaderText).SequenceEqual(expectedHeaders), "Exact grid headers and order");
            Require(grid.ReadOnly && !grid.AllowUserToAddRows && !grid.AllowUserToDeleteRows && grid.SelectionMode == DataGridViewSelectionMode.FullRowSelect, "Read-only full-row grid");
            Require(grid.Rows.Count == 10 && grid.Rows.Cast<DataGridViewRow>().Select(row => row.Cells[0].Value.ToString()).SequenceEqual(store.Load().Records.Select(record => record.Animal.Id)), "Startup file order and starter records");
            Require(FindButton(form, "btnAdd").Enabled && !FindButton(form, "btnSaveChanges").Enabled && !inputs["Id"].ReadOnly, "Initial Add mode");
            grid.ClearSelection();
            FindButton(form, "btnDelete").PerformClick();
            Require(confirmed is null && FindLabel(form, "statusMessage").Text.Contains("Select an animal") && File.ReadAllText(store.DataPath) == original, "Delete with no selection reports a message without writing");
            Fill(inputs, " wr-0011 ", "", "Serval", "3", "39");
            FindButton(form, "btnAdd").PerformClick();
            Require(FindLabel(form, "editorFeedback").Text.Contains("Name") && inputs["Species"].Text == "Serval" && File.ReadAllText(store.DataPath) == original, "Invalid Add identifies the field and preserves inputs/data");
            inputs["Name"].Text = "Kaya";
            inputs["Id"].Text = "wr-0001";
            FindButton(form, "btnAdd").PerformClick();
            Require(FindLabel(form, "editorFeedback").Text.Contains("already exists") && inputs["Name"].Text == "Kaya" && File.ReadAllText(store.DataPath) == original, "Duplicate Add preserves all data and entered fields");
            inputs["Id"].Text = "WR-0011";
            File.SetAttributes(store.DataPath, FileAttributes.ReadOnly);
            try
            {
                FindButton(form, "btnAdd").PerformClick();
                Require(FindLabel(form, "editorFeedback").Text.Contains("read-only") && inputs["Name"].Text == "Kaya" && File.ReadAllText(store.DataPath) == original, "Read-only Add is friendly and preserves the draft");
                Snapshot(form, "animal-error.png", output);
            }
            finally { File.SetAttributes(store.DataPath, FileAttributes.Normal); }
            using (var locked = new FileStream(store.DataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                FindButton(form, "btnAdd").PerformClick();
                Require(FindLabel(form, "editorFeedback").Text.Contains("locked") && inputs["Name"].Text == "Kaya", "Locked-file Add keeps the program and draft available");
            }
            Snapshot(form, "animal-add.png", output);
            FindButton(form, "btnAdd").PerformClick();
            var added = store.Load().Records.Last().Animal;
            Require(added == new Animal("WR-0011", "Kaya", "Serval", 3, 39) && added.Status == "Serious" && added.HousingUnit == "High-Dependency Ward", "Add saves all fields and score-39 classification");
            Require(File.ReadAllText(store.DataPath).StartsWith(original) && grid.Rows.Count == 11, "Add preserves existing lines and refreshes the grid once");
            Require(EmptyInputs(inputs) && FindButton(form, "btnAdd").Enabled && !FindButton(form, "btnSaveChanges").Enabled, "Successful Add clears the same inputs ready for another animal");
            Require(FindLabel(form, "editorFeedback").Text.Contains("Serious / High-Dependency Ward"), "Successful Add shows calculated classification");
            inputs["Name"].Text = "unsaved draft";
            inputs["searchId"].Text = "WR-9999";
            FindButton(form, "btnSearch").PerformClick();
            Require(inputs["Name"].Text == "unsaved draft" && grid.Rows.Count == 11 && FindLabel(form, "statusMessage").Text.Contains("not found"), "Missing search leaves the draft and grid unchanged");
            FindButton(form, "btnClear").PerformClick();
            inputs["searchId"].Text = " wr-0011 ";
            FindButton(form, "btnSearch").PerformClick();
            Require(inputs["Id"].ReadOnly && inputs["Name"].Text == "Kaya" && !FindButton(form, "btnAdd").Enabled && FindButton(form, "btnSaveChanges").Enabled, "Case-insensitive exact Search loads the shared inputs in edit mode");
            inputs["RecoveryScore"].Text = "79";
            Snapshot(form, "animal-edit.png", output);
            FindButton(form, "btnSaveChanges").PerformClick();
            Require(store.Load().Records.Last().Animal.Status == "Recovering" && EmptyInputs(inputs) && FindButton(form, "btnAdd").Enabled, "Search Update saves and returns to empty Add mode");
            ClickRow(grid, "WR-0011");
            Require(editor.EditingId == "WR-0011" && inputs["Id"].ReadOnly && FindButton(form, "btnSaveChanges").Enabled, "A grid row click loads the same edit inputs");
            inputs["RecoveryScore"].Text = "80";
            FindButton(form, "btnSaveChanges").PerformClick();
            Require(store.Load().Records.Last().Animal.Status == "Release-Ready" && store.Load().Records.Last().Animal.HousingUnit == "Pre-Release Camp", "Grid Update crosses the 79-to-80 classification boundary");
            Require(File.ReadAllText(store.DataPath).StartsWith(original) && grid.Rows.Count == 11, "Updates preserve unrelated records without duplicate rows");
            var beforeClear = File.ReadAllText(store.DataPath);
            ClickRow(grid, "WR-0011");
            inputs["Name"].Text = "discard this edit";
            FindButton(form, "btnClear").PerformClick();
            Require(EmptyInputs(inputs) && !inputs["Id"].ReadOnly && FindButton(form, "btnAdd").Enabled && !FindButton(form, "btnSaveChanges").Enabled && File.ReadAllText(store.DataPath) == beforeClear, "Clear leaves edit mode without saving a draft");
            ClickRow(grid, "WR-0011");
            FindButton(form, "btnDelete").PerformClick();
            Require(confirmed?.Id == "WR-0011" && confirmed.Name == "Kaya" && File.ReadAllText(store.DataPath) == beforeClear && editor.EditingId == "WR-0011", "Declining deletion changes nothing and confirms the correct ID/name");
            allowDelete = true;
            FindButton(form, "btnDelete").PerformClick();
            Require(File.ReadAllText(store.DataPath) == original && grid.Rows.Count == 10 && EmptyInputs(inputs) && editor.EditingId is null, "Confirming deletion removes only that animal and clears its loaded form");
            using (var locked = new FileStream(store.DataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                FindButton(form, "btnRefresh").PerformClick();
                Require(grid.Rows.Count == 0 && FindLabel(form, "statusMessage").Text.Contains("locked"), "A load failure is reported without crashing");
            }
            FindButton(form, "btnRefresh").PerformClick();
            Require(grid.Rows.Count == 10, "Refresh recovers after the file lock is released");
            var separator = original.EndsWith('\n') || original.EndsWith('\r') ? "" : Environment.NewLine;
            File.WriteAllText(store.DataPath, original + separator + "malformed\n\n");
            FindButton(form, "btnRefresh").PerformClick();
            Require(grid.Rows.Count == 10 && FindLabel(form, "statusMessage").Text.Contains("Skipped lines: 2."), "Malformed and blank lines are skipped and reported once");
            File.WriteAllText(store.DataPath, original);
            FindButton(form, "btnRefresh").PerformClick();
        });
        Render(new MainForm(fixtures), new Size(980, 550), "animal-records-compact.png", output);
        Render(new MainMenuForm(store), new Size(390, 550), "main-menu.png", output, form =>
        {
            var names = Descendants(form).OfType<Button>().Select(button => button.Name).ToArray();
            Require(new[] { "btnAddAnimal", "btnViewAnimals", "btnUpdateAnimal", "btnRemoveAnimal", "btnSummaryReport" }.All(names.Contains), "Nkosinathi's Main Menu retains all five feature commands");
            Require(Descendants(form).OfType<PictureBox>().Single().Image is not null && Descendants(form).OfType<Label>().Single(label => label.Name == "lblLoadStatus").Text.Contains("10 animals loaded"), "Main Menu retains its supplied artwork and loads current file counts");
        });
        Render(new MainForm(store, AnimalViewMode.View), new Size(1040, 550), "animal-view.png", output, form =>
        {
            Require(!Descendants(form).OfType<AnimalEditorPanel>().Single().Visible && Descendants(form).OfType<DataGridView>().Single().Rows.Count == 10, "The View-only page keeps its read-only file snapshot without edit inputs");
            Require(FindButton(form, "btnBack").Visible && !FindButton(form, "btnDelete").Visible, "View-only keeps Main Menu navigation and hides destructive actions");
        });
        Render(new SummaryForm(store), new Size(800, 560), "animal-summary.png", output, form =>
        {
            var report = Descendants(form).OfType<RichTextBox>().Single();
            Require(report.ReadOnly && report.Text.Contains("Total animals:            10") && File.ReadAllText(store.SummaryPath) == report.Text, "Summary is visible, read-only, and identical to its saved file");
            var generate = Descendants(form).OfType<Button>().Single(button => button.Text.Contains("Generate again"));
            File.WriteAllText(store.SummaryPath, "outdated report");
            generate.PerformClick();
            Require(File.ReadAllText(store.SummaryPath) == report.Text && !report.Text.Contains("Skipped"), "Summary generation overwrites the file using only the required layout");
            File.SetAttributes(store.SummaryPath, FileAttributes.ReadOnly);
            try
            {
                generate.PerformClick();
                Require(report.Text.Contains("Total animals:            10") && Descendants(form).OfType<Label>().Any(label => label.Text.Contains("read-only")), "Summary remains visible when its output file cannot be saved");
            }
            finally { File.SetAttributes(store.SummaryPath, FileAttributes.Normal); }
            generate.PerformClick();
            Require(File.ReadAllText(store.SummaryPath) == report.Text, "Summary saving recovers after permission correction");
        });
        var empty = Path.Combine(fixtures, "empty");
        Directory.CreateDirectory(empty);
        Render(new MainForm(empty), new Size(980, 550), "animal-empty.png", output, form =>
        {
            Require(File.Exists(Path.Combine(empty, "animals.txt")) && Descendants(form).OfType<DataGridView>().Single().Rows.Count == 0, "Missing file starts with an empty register");
            Require(!FindButton(form, "btnSaveChanges").Enabled, "Save Changes is unavailable when no animal is loaded");
        });
        Render(new SummaryForm(new AnimalFileStore(empty)), new Size(800, 560), "animal-summary-empty.png", output, form =>
        {
            var text = Descendants(form).OfType<RichTextBox>().Single().Text;
            Require(text.Contains("Total animals:            0") && text.Contains("Average age (years):      N/A") && text.Contains("Average recovery score:   N/A"), "Empty summary has zero counts and N/A averages");
        });
        Console.WriteLine($"{checks} real-form checks passed. Previews: {output}");
    }

    // Fails with the actual UI contract rather than silently accepting a rendered screenshot.
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        checks++;
    }

    // Finds a named command on the actual form.
    private static Button FindButton(Control form, string name) => Descendants(form).OfType<Button>().Single(button => button.Name == name);

    // Finds a named feedback surface on the actual form.
    private static Label FindLabel(Control form, string name) => Descendants(form).OfType<Label>().Single(label => label.Name == name);

    // Fills the same five textboxes used by both Add and Update.
    private static void Fill(Dictionary<string, TextBox> fields, string id, string name, string species, string age, string score)
    {
        fields["Id"].Text = id;
        fields["Name"].Text = name;
        fields["Species"].Text = species;
        fields["Age"].Text = age;
        fields["RecoveryScore"].Text = score;
    }

    // Verifies successful saves or Clear leave all five animal inputs empty.
    private static bool EmptyInputs(Dictionary<string, TextBox> fields) =>
        new[] { "Id", "Name", "Species", "Age", "RecoveryScore" }.All(name => fields[name].Text.Length == 0);

    // Raises a cell-click event on our own control, without sending desktop mouse input.
    private static void ClickRow(DataGridView grid, string id)
    {
        var row = grid.Rows.Cast<DataGridViewRow>().Single(row => row.Cells[0].Value.ToString() == id);
        grid.ClearSelection();
        grid.CurrentCell = row.Cells[0];
        row.Selected = true;
        typeof(DataGridView).GetMethod("OnCellClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(grid, new object[] { new DataGridViewCellEventArgs(0, row.Index) });
    }

    // Hosts a real form offscreen, renders it, and exercises its controls.
    private static void Render(Form form, Size size, string name, string output, Action<Form>? check = null)
    {
        using (form)
        using (var host = new PreviewHost())
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            host.ClientSize = size;
            host.Controls.Add(form);
            host.Show();
            form.Show();
            Application.DoEvents();
            Snapshot(form, name, output);
            check?.Invoke(form);
            host.Hide();
        }
    }

    // Saves the actual current view and checks that its inputs and actions fit their containers.
    private static void Snapshot(Form form, string name, string output)
    {
        Application.DoEvents();
        foreach (var control in Descendants(form).Where(control => control.Visible && control is Button or TextBox))
        {
            if (control.Right > control.Parent!.ClientSize.Width + 1 || control.Bottom > control.Parent.ClientSize.Height + 1 || control.Left < 0)
                throw new Exception("An input or action is clipped: " + control.Name);
        }
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        bitmap.Save(Path.Combine(output, name), ImageFormat.Png);
    }

    // Enumerates controls belonging to the test's own form.
    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private sealed class PreviewHost : Form
    {
        // Keeps preview windows offscreen and prevents activation or taskbar entries.
        public PreviewHost()
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-30000, -30000);
        }
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get { var parameters = base.CreateParams; parameters.ExStyle |= 0x08000000; return parameters; }
        }
    }

    private sealed class BackdropProbe : WindowsBackdropForm
    {
        protected override bool ShowWithoutActivation => true;
    }
}
