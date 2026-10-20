using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

public sealed class SummaryForm : WindowsBackdropForm
{
    private readonly AnimalFileStore store;
    private readonly RichTextBox rtbSummary = new() { Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, Font = new Font("Consolas", 10F), BackColor = SystemColors.Window, AccessibleName = "Animal summary report" };
    private readonly Label feedback = new() { AutoSize = true, Margin = new Padding(0, 10, 0, 10) };

    // Creates a read-only report surface and generates the first on-screen and saved report.
    public SummaryForm(AnimalFileStore store)
    {
        this.store = store;
        Text = "Animal summary report";
        DesktopTheme.Apply(this);
        ClientSize = new Size(800, 560);
        MinimumSize = new Size(650, 450);
        StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { BackColor = DesktopTheme.Canvas, Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var generate = new DesktopButton { Text = "&Generate again" };
        generate.Click += btnSummary_Click;
        var close = new DesktopButton { Text = "Close", DialogResult = DialogResult.Cancel };
        DesktopTheme.Style(generate);
        DesktopTheme.Style(close);
        buttons.Controls.Add(close);
        buttons.Controls.Add(generate);
        rtbSummary.BorderStyle = BorderStyle.None;
        rtbSummary.ForeColor = DesktopTheme.Ink;
        var paper = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.Fixed3D, BackColor = DesktopTheme.Surface, Padding = new Padding(12) };
        paper.Controls.Add(rtbSummary);
        layout.Controls.Add(paper, 0, 0);
        layout.Controls.Add(feedback, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        Controls.Add(layout);
        CancelButton = close;
        layout.SizeChanged += (_, _) => feedback.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 36), 0);
        Shown += btnSummary_Click;
    }

    // Adapted from Aseza Patiwe's frmSummaryReport.btnSummary_Click; totals live in AnimalSummary.Create.
    private void btnSummary_Click(object? sender, EventArgs e)
    {
        try
        {
            var document = store.Load();
            rtbSummary.Text = AnimalSummary.Create(document, DateTime.Now);
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            rtbSummary.Clear();
            feedback.ForeColor = DesktopTheme.Error;
            feedback.Text = "The report could not be generated. " + UiErrors.Describe(exception);
            return;
        }
        try
        {
            store.SaveSummary(rtbSummary.Text);
            feedback.ForeColor = DesktopTheme.Muted;
            feedback.Text = "Report generated and saved to " + store.SummaryPath;
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            feedback.ForeColor = DesktopTheme.Error;
            feedback.Text = "The report is visible above, but summary.txt could not be saved. " + UiErrors.Describe(exception);
        }
    }
}
