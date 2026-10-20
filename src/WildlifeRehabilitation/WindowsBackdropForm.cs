using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace WildlifeRehabilitation;

// Keep the acrylic in the frame. Native input controls and the record grid stay opaque.
public class WindowsBackdropForm : Form
{
    public bool NativeBackdropActive { get; private set; }

    // Reserves an outer frame for the optional native acrylic backdrop.
    public WindowsBackdropForm() => Padding = new Padding(12);

    // Enables supported Windows 11 acrylic and falls back safely on older/high-contrast systems.
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        NativeBackdropActive = false;
        BackColor = DesktopTheme.Canvas;
        if (!TopLevel || SystemInformation.HighContrast || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621)) return;
        var backdrop = 3; // DWMSBT_TRANSIENTWINDOW: Windows desktop acrylic.
        var lightTitleBar = 0;
        var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        if (DwmSetWindowAttribute(Handle, 38, ref backdrop, sizeof(int)) >= 0 &&
            DwmExtendFrameIntoClientArea(Handle, ref margins) >= 0)
        {
            DwmSetWindowAttribute(Handle, 20, ref lightTitleBar, sizeof(int));
            NativeBackdropActive = true;
            BackColor = Color.Black;
        }
        else
        {
            var none = 1;
            DwmSetWindowAttribute(Handle, 38, ref none, sizeof(int));
        }
    }

    // Paints the DWM-compatible frame or the opaque fallback without making inputs transparent.
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (NativeBackdropActive)
        {
            // DWM glass needs zero-alpha black pixels, supplied by the native GDI brush.
            var bounds = new NativeRectangle { Left = 0, Top = 0, Right = ClientSize.Width, Bottom = ClientSize.Height };
            var context = e.Graphics.GetHdc();
            try { FillRect(context, ref bounds, GetStockObject(4)); }
            finally { e.Graphics.ReleaseHdc(context); }
            return;
        }
        if (SystemInformation.HighContrast || ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
        {
            base.OnPaintBackground(e);
            return;
        }
        using var brush = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(224, 238, 235), Color.FromArgb(238, 232, 244), 32F);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins { public int Left, Right, Top, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle { public int Left, Top, Right, Bottom; }

    // Gets Windows' black brush for the zero-alpha DWM frame.
    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern IntPtr GetStockObject(int objectIndex);

    // Fills the native frame rectangle without painting over child controls.
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int FillRect(IntPtr context, ref NativeRectangle rectangle, IntPtr brush);

    // Requests a documented Windows desktop-window attribute.
    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

    // Extends the compositor's frame into the form's reserved border.
    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr window, ref Margins margins);
}
