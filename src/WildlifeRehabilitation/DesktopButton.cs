using System.Drawing.Drawing2D;

namespace WildlifeRehabilitation;

// Retain Button's keyboard, default-action, and accessibility behavior; customize only its paint.
internal sealed class DesktopButton : Button
{
    private bool hovered;
    private bool pressed;
    public bool Primary { get; set; }

    public DesktopButton() => SetStyle(ControlStyles.AllPaintingInWmPaint |
        ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

    public override Size GetPreferredSize(Size proposedSize)
    {
        var size = base.GetPreferredSize(proposedSize);
        return new Size(size.Width, size.Height + 4);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
        var graphics = e.Graphics;
        graphics.Clear(Parent?.BackColor ?? DesktopTheme.Canvas);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var face = new Rectangle(1, pressed ? 3 : 1, Width - 3, Height - 6);
        if (face.Width <= 0 || face.Height <= 0) return;
        if (Enabled && !pressed)
        {
            using var shadow = Rounded(new Rectangle(face.X, face.Y + 2, face.Width, face.Height), 4);
            using var shadowBrush = new SolidBrush(Color.FromArgb(38, 24, 42, 49));
            graphics.FillPath(shadowBrush, shadow);
        }
        var top = Primary ? Color.FromArgb(24, 133, 121) : Color.White;
        var bottom = Primary ? Color.FromArgb(6, 100, 92) : Color.FromArgb(230, 235, 239);
        if (hovered) { top = ControlPaint.Light(top, .08F); bottom = ControlPaint.Light(bottom, .08F); }
        if (pressed) { top = bottom; bottom = ControlPaint.Dark(bottom, .08F); }
        if (!Enabled) top = bottom = Color.FromArgb(237, 240, 242);
        using var outline = Rounded(face, 4);
        using var fill = new LinearGradientBrush(face, top, bottom, LinearGradientMode.Vertical);
        graphics.FillPath(fill, outline);
        using var border = new Pen(Primary && Enabled ? Color.FromArgb(5, 87, 80) : Color.FromArgb(177, 189, 197));
        graphics.DrawPath(border, outline);
        if (!pressed)
        {
            using var highlight = new Pen(Color.FromArgb(Primary ? 60 : 200, Color.White));
            graphics.DrawLine(highlight, face.Left + 5, face.Top + 1, face.Right - 5, face.Top + 1);
        }
        var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
        if (!ShowKeyboardCues) flags |= TextFormatFlags.HidePrefix;
        TextRenderer.DrawText(graphics, Text, Font, face, Enabled ? ForeColor : Color.FromArgb(124, 137, 145), flags);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(graphics, Rectangle.Inflate(face, -5, -5), ForeColor, top);
    }

    private static GraphicsPath Rounded(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnMouseEnter(EventArgs e) { hovered = true; base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { hovered = pressed = false; base.OnMouseLeave(e); Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed = e.Button == MouseButtons.Left; base.OnMouseDown(e); Invalidate(); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; base.OnMouseUp(e); Invalidate(); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) pressed = true; base.OnKeyDown(e); Invalidate(); }
    protected override void OnKeyUp(KeyEventArgs e) { pressed = false; base.OnKeyUp(e); Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { pressed = false; base.OnLostFocus(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
}
