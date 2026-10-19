using WildlifeRehabilitation.Domain;
using WildlifeRehabilitation.Storage;

namespace WildlifeRehabilitation;

public sealed class SummaryForm : Form
{
    private readonly AnimalFileStore store;
    private readonly RichTextBox report = new() { Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, Font = new Font("Consolas", 10F), BackColor = SystemColors.Window, AccessibleName = "Animal summary report" };
    private readonly Label feedback = new() { AutoSize = true, Margin = new Padding(0, 10, 0, 10) };

    public SummaryForm(AnimalFileStore store)
    {
        this.store = store;
        Text = "Animal summary report";
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(850, 600);
        MinimumSize = new Size(650, 500);
        StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        var generate = new Button { Text = "&Generate again", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        generate.Click += (_, _) => Generate();
        var close = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        buttons.Controls.Add(generate);
        buttons.Controls.Add(close);
        layout.Controls.Add(report, 0, 0);
        layout.Controls.Add(feedback, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        Controls.Add(layout);
        CancelButton = close;
        layout.SizeChanged += (_, _) => feedback.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - 45), 0);
        Shown += (_, _) => Generate();
    }

    private void Generate()
    {
        try
        {
            var document = store.Load();
            report.Text = AnimalSummary.Create(document, DateTime.Now);
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            report.Clear();
            feedback.ForeColor = Color.Maroon;
            feedback.Text = "The report could not be generated. " + UiErrors.Describe(exception);
            return;
        }
        try
        {
            store.SaveSummary(report.Text);
            feedback.ForeColor = SystemColors.ControlText;
            feedback.Text = "Report generated and saved to " + store.SummaryPath;
        }
        catch (Exception exception) when (UiErrors.IsFileError(exception))
        {
            feedback.ForeColor = Color.Maroon;
            feedback.Text = "The report is visible above, but summary.txt could not be saved. " + UiErrors.Describe(exception);
        }
    }
}
