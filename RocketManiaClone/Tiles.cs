using System.Windows.Media;
using System.Windows;

namespace RocketManiaClone
{
    public class ITile : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, -1, 74, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, 50, 74, 21));
        }
    }

    public class LTile : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Point origin = new Point(0, 0);
            drawingContext.DrawEllipse(Brushes.Transparent, new Pen(Brushes.DarkSeaGreen, 60), origin, 80, 80);
            drawingContext.DrawEllipse(Brushes.Transparent, App.blackPen, origin, 50, 50);
            drawingContext.DrawEllipse(Brushes.DarkSeaGreen, App.blackPen, origin, 20, 20);
        }
    }

    public class TTile : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, -1, 21, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(50, -1, 21, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, 50, 74, 21));
        }
    }

    public class XTile : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, -1, 21, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(50, -1, 21, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(-1, 50, 21, 21));
            drawingContext.DrawRectangle(Brushes.DarkSeaGreen, App.blackPen, new Rect(50, 50, 21, 21));
        }
    }
}
