using CodeCloud.VisualStudio.Shared.Controls;
using System.Collections.Generic;
using System.Windows.Media;

namespace CodeCloud.VisualStudio.Shared
{
    public static class SharedResources
    {
        static readonly Dictionary<string, DrawingBrush> drawingBrushes = new Dictionary<string, DrawingBrush>();

        public static DrawingBrush GetDrawingForIcon(Octicon icon, Color color, string theme = null)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            return GetDrawingForIcon(icon, brush, theme);
        }

        public static DrawingBrush GetDrawingForIcon(Octicon icon, Brush colorBrush, string theme = null)
        {
            string name = icon.ToString();
            if (theme != null)
                name += "_" + theme;
            if (drawingBrushes.ContainsKey(name))
                return drawingBrushes[name];

            var geometry = OcticonPath.GetGeometryForIcon(icon);
            geometry.Freeze();

            var drawing = new GeometryDrawing()
            {
                Brush = colorBrush,
                Geometry = geometry
            };
            drawing.Freeze();

            var brush = new DrawingBrush()
            {

                Drawing = drawing,
                Stretch = Stretch.Uniform
            };
            brush.Freeze();

            drawingBrushes.Add(name, brush);
            return brush;
        }
    }
}
