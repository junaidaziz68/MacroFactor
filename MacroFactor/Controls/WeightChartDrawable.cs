using Microsoft.Maui.Graphics;
using MacroFactor.Models;

namespace MacroFactor.Controls;

public class WeightChartDrawable : IDrawable
{
    public List<WeightLog> Data { get; set; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        if (Data == null || Data.Count == 0)
        {
            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 14;
            canvas.DrawString("No data yet to graph.", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var padding = 20f;
        var drawWidth = width - (padding * 2);
        var drawHeight = height - (padding * 2);

        var maxWeight = Data.Max(w => w.Weight) + 5;
        var minWeight = Data.Min(w => w.Weight) - 5;
        if (maxWeight == minWeight) maxWeight += 10;

        var weightRange = maxWeight - minWeight;
        var pointSpacing = Data.Count > 1 ? drawWidth / (Data.Count - 1) : drawWidth;

        var path = new PathF();
        var points = new List<PointF>();

        for (int i = 0; i < Data.Count; i++)
        {
            var log = Data[i];
            var x = padding + (i * pointSpacing);
            var yRatio = (float)((log.Weight - minWeight) / weightRange);
            var y = padding + drawHeight - (yRatio * drawHeight);
            
            points.Add(new PointF(x, y));

            if (i == 0)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }

        // Draw Line
        canvas.StrokeColor = Color.FromArgb("#10B981");
        canvas.StrokeSize = 3;
        canvas.DrawPath(path);

        // Draw Points
        canvas.FillColor = Color.FromArgb("#34D399");
        foreach (var p in points)
        {
            canvas.FillCircle(p, 5);
        }
    }
}
