using System.Drawing.Imaging;
using WildlifeRehabilitation;
using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

internal static class Program
{
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

        Render(new MainMenuForm(store), new Size(390, 550), "main-menu.png", output, form =>
        {
            if (Descendants(form).OfType<Button>().Count() != 5 || Descendants(form).OfType<DataGridView>().Any())
                throw new Exception("Startup must show the five-button Main Menu before the animal grid.");
            if (!Descendants(form).OfType<Label>().Any(label => label.Text.Contains("10 animals loaded. Skipped lines: 0.")))
                throw new Exception("The Main Menu did not load the file at startup.");
            CheckMenuNavigation(form, "btnViewAnimals", typeof(MainForm), "View all animals");
            CheckMenuNavigation(form, "btnUpdateAnimal", typeof(MainForm), "Update an animal");
            CheckMenuNavigation(form, "btnRemoveAnimal", typeof(MainForm), "Remove an animal");
            CheckMenuNavigation(form, "btnAddAnimal", typeof(AnimalEditorForm), "Add animal");
            CheckMenuNavigation(form, "btnSummaryReport", typeof(SummaryForm), "Animal summary report");
        });
        Render(new MainForm(store), new Size(1040, 480), "animal-records.png", output, form =>
        {
            var grid = Descendants(form).OfType<DataGridView>().Single();
            string[] headers = ["Animal ID", "Name", "Species", "Age", "Recovery Score", "Status", "Housing Unit"];
            if (grid.Rows.Count != 10 || !grid.ReadOnly || grid.AllowUserToAddRows || grid.AllowUserToDeleteRows ||
                grid.MultiSelect || grid.SelectionMode != DataGridViewSelectionMode.FullRowSelect ||
                !grid.Columns.Cast<DataGridViewColumn>().Select(column => column.HeaderText).SequenceEqual(headers) ||
                grid.Columns.Cast<DataGridViewColumn>().Any(column => column.SortMode != DataGridViewColumnSortMode.NotSortable))
                throw new Exception("The animal grid must have seven ordered, read-only columns and full-row selection.");
            var refresh = Descendants(form).OfType<Button>().Single(button => button.Name == "btnRefresh");
            var original = File.ReadAllText(store.DataPath);
            var extra = new Animal("WR-0088", "Refresh test", "Serval", 3, 79);
            store.Append(extra);
            refresh.PerformClick();
            refresh.PerformClick();
            if (grid.Rows.Count != 11 || grid.Rows[10].Cells[0].Value?.ToString() != extra.Id)
                throw new Exception("Refreshing after Add must replace the grid without duplicates and preserve file order.");
            grid.CurrentCell = grid.Rows[10].Cells[0];
            store.Update(store.Load().Records.Last(), extra with { RecoveryScore = 80 });
            refresh.PerformClick();
            if (grid.Rows.Count != 11 || grid.CurrentRow?.Cells[0].Value?.ToString() != extra.Id ||
                grid.Rows[10].Cells[5].Value?.ToString() != "Release-Ready")
                throw new Exception("Refreshing after Update must keep the selected ID and show its new classification.");
            store.Delete(store.Load().Records.Last());
            refresh.PerformClick();
            if (grid.Rows.Count != 10 || grid.CurrentRow != null)
                throw new Exception("Refreshing after Delete must remove the row and clear a deleted selection.");
            File.WriteAllText(store.DataPath, "malformed\n" + original);
            refresh.PerformClick();
            if (grid.Rows.Count != 10 || !Descendants(form).OfType<Label>().Any(label => label.Text.Contains("Skipped lines: 1.")))
                throw new Exception("The view must display one skipped-line count and exclude malformed records.");
            File.Delete(store.DataPath);
            refresh.PerformClick();
            if (grid.Rows.Count != 0 || File.ReadAllText(store.DataPath) != "")
                throw new Exception("A missing file must produce an empty grid and an empty file.");
            File.WriteAllText(store.DataPath, original);
            refresh.PerformClick();
        });
        Render(new MainForm(store), new Size(980, 400), "animal-records-compact.png", output);
        Render(new MainForm(store, AnimalViewMode.Update), new Size(980, 400), "animal-update-selection.png", output);
        Render(new MainForm(store, AnimalViewMode.Delete), new Size(980, 400), "animal-delete-selection.png", output);
        Animal? saved = null;
        Render(new AnimalEditorForm("Add animal", new AnimalDraft("WR-0011", "Kaya", "Serval", "3", "39"), animal =>
        {
            store.Append(animal);
            saved = animal;
        }),
            new Size(590, 430), "animal-editor.png", output, form =>
        {
            if (form.AcceptButton is not Button save) throw new Exception("The editor needs a keyboard default Save action.");
            var inputs = Descendants(form).OfType<TextBox>().ToDictionary(field => field.Name);
            var before = File.ReadAllText(store.DataPath);
            inputs["Name"].Text = "";
            save.PerformClick();
            if (saved != null || !form.Visible || inputs["Species"].Text != "Serval" || File.ReadAllText(store.DataPath) != before)
                throw new Exception("Invalid Add must preserve the form, other inputs, and file contents.");
            inputs["Name"].Text = "Kaya";
            inputs["Id"].Text = store.Load().Records[0].Animal.Id;
            save.PerformClick();
            if (saved != null || !form.Visible || inputs["Name"].Text != "Kaya" || File.ReadAllText(store.DataPath) != before ||
                !Descendants(form).OfType<Label>().Any(label => label.Text.Contains("already exists")))
                throw new Exception("Duplicate Add must report the conflict and preserve the entered fields and file.");
            inputs["Id"].Text = "WR-0011";
            save.PerformClick();
            if (saved != new Animal("WR-0011", "Kaya", "Serval", 3, 39) || saved?.Status != "Serious" ||
                store.Load().Records.Last().Animal != saved || form.DialogResult != DialogResult.OK)
                throw new Exception("The rendered Add handler did not save the five fields and score-39 classification to disk.");
        });
        var selected = store.Load().Records.Last();
        Render(new AnimalEditorForm("Update WR-0011", new AnimalDraft("WR-0011", "Kaya", "Serval", "3", "39"),
            animal => store.Update(selected, animal), lockId: true),
            new Size(590, 430), "animal-editor-update.png", output, form =>
        {
            var inputs = Descendants(form).OfType<TextBox>().ToDictionary(field => field.Name);
            if (!inputs["Id"].ReadOnly) throw new Exception("Update must keep the selected ID read-only.");
            inputs["Name"].Text = "Kaya updated";
            inputs["RecoveryScore"].Text = "40";
            ((Button)form.AcceptButton!).PerformClick();
            var updated = store.Load().Records.Single(record => record.Animal.Id == "WR-0011").Animal;
            if (updated.Name != "Kaya updated" || updated.Status != "Stable" || store.Load().Records.Count != 11)
                throw new Exception("The shared editor did not update the selected animal using the shared classification.");
        });
        Render(new SummaryForm(store), new Size(800, 560), "animal-summary.png", output, form =>
        {
            var report = Descendants(form).OfType<RichTextBox>().Single();
            if (!report.Text.Contains("Total animals: 11") || !File.Exists(store.SummaryPath) || File.ReadAllText(store.SummaryPath) != report.Text)
                throw new Exception("The rendered summary did not generate and save its report.");
            File.WriteAllText(store.SummaryPath, "outdated report");
            Descendants(form).OfType<Button>().Single(button => button.Text.Contains("Generate again")).PerformClick();
            if (!report.Text.Contains("Total animals: 11") || File.ReadAllText(store.SummaryPath) != report.Text)
                throw new Exception("The summary button did not regenerate matching on-screen and saved reports.");
        });
        Console.WriteLine("Main Menu navigation, read-only View, refresh after Add/Update/Delete, malformed and missing files, Add validation, Update, and Summary form checks passed.");
    }

    // Exercises a real menu button and uses the opened form's Back/Cancel action to return.
    private static void CheckMenuNavigation(Form menu, string buttonName, Type expectedType, string expectedTitle)
    {
        Exception? failure = null;
        var closed = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 50 };
        timer.Tick += (_, _) =>
        {
            var dialog = menu.OwnedForms.FirstOrDefault(form => form.Visible);
            if (dialog == null) return;
            timer.Stop();
            if (dialog.GetType() != expectedType || dialog.Text != expectedTitle)
                failure = new Exception("The menu button opened the wrong feature: " + buttonName);
            if (dialog.CancelButton is Button back) back.PerformClick();
            else dialog.Close();
            closed = true;
        };
        timer.Start();
        Descendants(menu).OfType<Button>().Single(button => button.Name == buttonName).PerformClick();
        timer.Stop();
        if (failure != null) throw failure;
        if (!closed || !menu.Visible) throw new Exception("Back must close only the feature and return to the Main Menu.");
    }

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
            foreach (var control in Descendants(form).Where(control => control.Visible && control is Button or TextBox))
            {
                if (control.Right > control.Parent!.ClientSize.Width + 1 || control.Bottom > control.Parent.ClientSize.Height + 1)
                    throw new Exception("An input or action is clipped: " + control.Text);
            }
            using var bitmap = new Bitmap(size.Width, size.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, size));
            bitmap.Save(Path.Combine(output, name), ImageFormat.Png);
            check?.Invoke(form);
            host.Hide();
        }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    // These are our own forms, rendered outside the visible desktop without activation or input injection.
    private sealed class PreviewHost : Form
    {
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
