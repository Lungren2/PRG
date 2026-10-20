using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

// Nkosinathi Mathenjwa's menu and navigation based on UserInterface.zip.
public sealed class MainMenuForm : Form
{
    private readonly AnimalFileStore store;
    private readonly PictureBox logo = new();
    private readonly Label feedback = new();

    // Uses the same executable-relative data folder as the existing feature forms.
    public MainMenuForm() : this(new AnimalFileStore(AppContext.BaseDirectory)) { }

    // Builds the supplied Main Menu and loads records after the menu becomes visible.
    public MainMenuForm(AnimalFileStore store)
    {
        this.store = store;
        Text = "Main Menu | Baobab Ridge";
        Name = "MainMenuForm";
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Arial", 10F);
        BackColor = Color.White;
        ForeColor = Color.Black;
        ClientSize = new Size(390, 550);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var heading = new Label { Text = "Animal Rescue center", AutoSize = false,
            Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 25), Size = new Size(350, 30) };
        Controls.Add(heading);
        logo.Location = new Point(100, 75);
        logo.Size = new Size(190, 190);
        logo.SizeMode = PictureBoxSizeMode.Zoom;
        logo.TabStop = false;
        // Crop the koala from the supplied menu screenshot to reuse the original artwork.
        using (var stream = typeof(MainMenuForm).Assembly.GetManifestResourceStream("WildlifeRehabilitation.Data.MainMenu.png")!)
        using (var suppliedMenu = new Bitmap(stream))
            logo.Image = suppliedMenu.Clone(new Rectangle(80, 65, 180, 175), suppliedMenu.PixelFormat);
        Controls.Add(logo);

        AddMenuButton("btnAddAnimal", "Add new animal", new Point(30, 305), (_, _) => OpenAnimals(AnimalViewMode.Add));
        AddMenuButton("btnViewAnimals", "View all animals", new Point(220, 305), (_, _) => OpenAnimals(AnimalViewMode.View));
        AddMenuButton("btnUpdateAnimal", "Update an animal", new Point(30, 375), (_, _) => OpenAnimals(AnimalViewMode.Update));
        AddMenuButton("btnRemoveAnimal", "Remove an animal", new Point(220, 375), (_, _) => OpenAnimals(AnimalViewMode.Delete));
        AddMenuButton("btnSummaryReport", "Summary report", new Point(30, 430), (_, _) => OpenSummary());
        feedback.Location = new Point(30, 490);
        feedback.Size = new Size(330, 50);
        feedback.Name = "lblLoadStatus";
        Controls.Add(feedback);
        Shown += (_, _) => LoadAnimals();
    }

    // Adds one of the five feature buttons using the supplied white-and-black style.
    private void AddMenuButton(string name, string text, Point location, EventHandler onClick)
    {
        var button = new Button { Name = name, Text = text, Location = location,
            Size = new Size(140, 42), FlatStyle = FlatStyle.Flat, BackColor = Color.White,
            ForeColor = Color.Black, UseVisualStyleBackColor = false };
        button.FlatAppearance.BorderColor = Color.Black;
        button.Click += onClick;
        Controls.Add(button);
    }

    // Reads the file at startup and after a feature closes; reports the skipped count once per load.
    private void LoadAnimals(string? message = null)
    {
        try
        {
            var document = store.Load();
            feedback.ForeColor = Color.Black;
            feedback.Text = (message == null ? "" : message + " ") +
                $"{document.Records.Count} animals loaded. Skipped lines: {document.SkippedCount}.";
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            feedback.ForeColor = Color.Maroon;
            feedback.Text = "Records could not be loaded. " + UiErrors.Describe(exception);
        }
    }

    // Opens the requested grid page and returns to this menu when Back or the close button is used.
    private void OpenAnimals(AnimalViewMode mode)
    {
        using var view = new MainForm(store, mode);
        view.ShowDialog(this);
        LoadAnimals();
    }

    // Opens on-screen and text-file summary report.
    private void OpenSummary()
    {
        using var summary = new SummaryForm(store);
        summary.ShowDialog(this);
        LoadAnimals();
    }

    // Releases the image loaded  when the menu closes.
    protected override void Dispose(bool disposing)
    {
        if (disposing) logo.Image?.Dispose();
        base.Dispose(disposing);
    }
}
