namespace WildlifeRehabilitation;

internal static class DesktopTheme
{
    public static Color Canvas => SystemInformation.HighContrast ? SystemColors.Control : Color.FromArgb(238, 242, 244);
    public static Color Surface => SystemColors.Window;
    public static Color Ink => SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(27, 43, 50);
    public static Color Muted => SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(88, 104, 114);
    public static Color Border => SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(216, 224, 229);
    public static Color Accent => SystemInformation.HighContrast ? SystemColors.Highlight : Color.FromArgb(8, 112, 101);
    public static Color Selected => SystemInformation.HighContrast ? SystemColors.Highlight : Color.FromArgb(224, 241, 236);
    public static Color Error => SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(155, 44, 44);
    public static readonly Font BodyFont = new("Segoe UI", 10F);
    public static readonly Font StrongFont = new("Segoe UI Semibold", 10F);

    // Applies the shared DPI, font, and high-contrast-aware colors to a form.
    public static void Apply(Form form)
    {
        form.AutoScaleDimensions = new SizeF(96F, 96F);
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.Font = BodyFont;
        form.BackColor = Canvas;
        form.ForeColor = Ink;
    }

    // Sets button sizing, colors, and native keyboard behavior consistently.
    public static void Style(Button button, bool primary = false, bool danger = false)
    {
        button.FlatStyle = SystemInformation.HighContrast ? FlatStyle.Standard : FlatStyle.Flat;
        if (button is DesktopButton desktopButton) desktopButton.Primary = primary;
        button.UseVisualStyleBackColor = false;
        button.BackColor = primary ? Accent : Surface;
        button.ForeColor = primary ? SystemColors.HighlightText : danger ? Error : Ink;
        button.Font = StrongFont;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = primary ? Accent : Border;
        button.FlatAppearance.MouseOverBackColor = SystemInformation.HighContrast ? SystemColors.Highlight :
            primary ? Color.FromArgb(6, 92, 84) : danger ? Color.FromArgb(255, 240, 239) : Color.FromArgb(234, 240, 243);
        button.FlatAppearance.MouseDownBackColor = primary ? Accent : Selected;
        button.AutoSize = true;
        button.MinimumSize = new Size(72, 36);
        button.Padding = new Padding(10, 5, 10, 5);
        button.Margin = new Padding(0, 0, 8, 4);
    }

    // Gives editable fields a recessed Windows border and readable text.
    public static void Style(TextBox field)
    {
        field.BorderStyle = BorderStyle.Fixed3D;
        field.BackColor = Surface;
        field.ForeColor = Ink;
        field.Font = new Font("Segoe UI", 11F);
    }

    // Styles the record grid while preserving native selection and scrolling.
    public static void Style(DataGridView grid)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Surface;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Canvas, ForeColor = Muted, Font = StrongFont,
            Padding = new Padding(12, 0, 12, 0), SelectionBackColor = Canvas, SelectionForeColor = Ink
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface, ForeColor = Ink, Font = BodyFont, Padding = new Padding(12, 0, 12, 0),
            SelectionBackColor = Selected, SelectionForeColor = SystemInformation.HighContrast ? SystemColors.HighlightText : Ink
        };
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.RowTemplate.Height = 34;
        grid.RowTemplate.MinimumHeight = 30;
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Status" || SystemInformation.HighContrast) return;
            e.CellStyle!.Font = StrongFont;
            e.CellStyle.ForeColor = e.Value?.ToString() switch
            {
                "Critical" => Color.FromArgb(155, 44, 44),
                "Serious" => Color.FromArgb(139, 82, 10),
                "Stable" => Color.FromArgb(51, 91, 130),
                "Recovering" => Color.FromArgb(8, 112, 101),
                _ => Color.FromArgb(37, 105, 62)
            };
        };
    }
}
