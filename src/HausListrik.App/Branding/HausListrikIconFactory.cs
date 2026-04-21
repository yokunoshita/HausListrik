using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HausListrik.App.Branding;

public static class HausListrikIconFactory
{
    public static Icon CreateTrayIcon()
    {
        using var bitmap = CreateBitmap(64);
        var handle = bitmap.GetHicon();

        try
        {
            using var temporaryIcon = Icon.FromHandle(handle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    public static ImageSource CreateWindowIcon()
    {
        using var icon = CreateTrayIcon();
        using var iconBitmap = icon.ToBitmap();

        var handle = iconBitmap.GetHicon();

        try
        {
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(64, 64));

            imageSource.Freeze();
            return imageSource;
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static Bitmap CreateBitmap(int size)
    {
        var bitmap = new Bitmap(size, size);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        var canvas = new RectangleF(6, 6, size - 12, size - 12);
        using var backgroundBrush = new SolidBrush(Color.FromArgb(255, 17, 21, 34));
        using var glowBrush = new SolidBrush(Color.FromArgb(255, 55, 118, 255));
        using var batteryFillBrush = new LinearGradientBrush(
            new PointF(canvas.Left, canvas.Top),
            new PointF(canvas.Right, canvas.Bottom),
            Color.FromArgb(255, 48, 216, 164),
            Color.FromArgb(255, 30, 176, 235));
        using var shellPen = new Pen(Color.FromArgb(255, 228, 237, 250), 3.4f)
        {
            LineJoin = LineJoin.Round
        };
        using var boltBrush = new SolidBrush(Color.FromArgb(255, 255, 201, 66));

        graphics.FillEllipse(backgroundBrush, canvas);
        graphics.FillEllipse(glowBrush, canvas.Left + 4, canvas.Top + 4, canvas.Width - 8, canvas.Height - 8);

        var batteryBody = new RectangleF(16, 18, 26, 28);
        var batteryCap = new RectangleF(42, 26, 6, 12);
        FillRoundedRectangle(graphics, batteryFillBrush, batteryBody, 6);
        FillRoundedRectangle(graphics, batteryFillBrush, batteryCap, 3);
        DrawRoundedRectangle(graphics, shellPen, batteryBody, 6);
        DrawRoundedRectangle(graphics, shellPen, batteryCap, 3);

        using var boltPath = new GraphicsPath();
        boltPath.AddPolygon(
        [
            new PointF(30, 16),
            new PointF(22, 31),
            new PointF(29, 31),
            new PointF(24, 48),
            new PointF(39, 28),
            new PointF(31, 28)
        ]);
        graphics.FillPath(boltBrush, boltPath);

        return bitmap;
    }

    private static void FillRoundedRectangle(Graphics graphics, Brush brush, RectangleF rectangle, float radius)
    {
        using var path = CreateRoundedRectanglePath(rectangle, radius);
        graphics.FillPath(brush, path);
    }

    private static void DrawRoundedRectangle(Graphics graphics, Pen pen, RectangleF rectangle, float radius)
    {
        using var path = CreateRoundedRectanglePath(rectangle, radius);
        graphics.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedRectanglePath(RectangleF rectangle, float radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
