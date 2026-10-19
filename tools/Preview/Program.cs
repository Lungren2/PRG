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
        store.Load();

        Render(new MainForm(), new Size(1120, 600), "animal-records.png", output, form =>
        {
            var grid = Descendants(form).OfType<DataGridView>().Single();
            if (grid.Rows.Count < 8 || !grid.ReadOnly) throw new Exception("The live records view did not load its starters.");
            if (!Descendants(form).OfType<Button>().Single(button => button.Text.Contains("Edit selected")).Enabled)
                throw new Exception("The selected row must be editable after initial loading.");
        });
        Render(new MainForm(), new Size(980, 500), "animal-records-compact.png", output);
        Animal? saved = null;
        Render(new AnimalEditorForm("Add animal", new AnimalDraft("WR-0011", "Kaya", "Serval", "3", "39"), animal => saved = animal),
            new Size(590, 430), "animal-editor.png", output, form =>
        {
            if (form.AcceptButton is not Button save) throw new Exception("The editor needs a keyboard default Save action.");
            save.PerformClick();
            if (saved?.Status != "Serious") throw new Exception("The rendered editor did not validate and save score 39.");
        });
        Render(new SummaryForm(store), new Size(800, 560), "animal-summary.png", output, form =>
        {
            var report = Descendants(form).OfType<RichTextBox>().Single();
            if (!report.Text.Contains("Total animals: 10") || !File.Exists(store.SummaryPath))
                throw new Exception("The rendered summary did not generate and save its report.");
        });
        Console.WriteLine("Rendered four previews. Live view, editor save, and summary form checks passed.");
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
