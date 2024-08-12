using Prototype.Main.ViewModels;
using Prototype.Main.VirtualCanvas.Interfaces;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Main.VirtualCanvas
{
    /// <summary>
    /// This is an even more light weight way to create the visuals by using drawingContext
    /// </summary>
    public class DemoShapeVisual : FrameworkElement, ISemanticZoomable
    {
        double scale = 1.0;

        public DemoShape Shape { get; set; }

        public bool Selected
        {
            get => Shape?.IsSelected == true;
            set
            {
                if (Shape != null)
                {
                    Shape.IsSelected = value;
                    InvalidateVisual();
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Pen selectionPen = null;
            Rect selectionBounds;
            if (Selected)
            {
                selectionBounds = Shape.Bounds;
                selectionBounds.Inflate(1.5, 1.5);
                selectionPen = new Pen(Brushes.Yellow, 3 / scale);
            }
            Pen pen = null;
            if (Shape.Stroke != null)
            {
                pen = new Pen(Shape.Stroke, Shape.StrokeThickness / scale);
            }

            switch (Shape.Type)
            {
                case ShapeType.Ellipse:
                    {
                        double xrad = Shape.Bounds.Width / 2;
                        double yrad = Shape.Bounds.Height / 2;
                        drawingContext.DrawEllipse(Shape.Fill, pen, new Point(xrad, yrad), xrad, yrad);

                        if (Selected)
                        {
                            drawingContext.DrawEllipse(null, selectionPen, new Point(xrad, yrad), xrad, yrad);
                        }

                    }
                    break;
                case ShapeType.Rect:
                    
                    drawingContext.DrawRectangle(Shape.Fill, pen, new Rect(0, 0, Shape.Bounds.Width, Shape.Bounds.Height));

                    if (Selected)
                    {
                        drawingContext.DrawRectangle(null, selectionPen, new Rect(-1.5, -1.5, selectionBounds.Width, selectionBounds.Height));
                    }
                    break;
                case ShapeType.RoundedRect:
                    {
                        var radiusX = Shape.Bounds.Width / 20;
                        var radiusY = Shape.Bounds.Height / 20;
                        drawingContext.DrawRoundedRectangle(Shape.Fill, pen, new Rect(0, 0, Shape.Bounds.Width, Shape.Bounds.Height), radiusX, radiusY);

                        if (Selected)
                        {
                            var selectionRadiusX = selectionBounds.Width / 20;
                            var selectionRadiusY = selectionBounds.Height / 20;
                            drawingContext.DrawRoundedRectangle(null, selectionPen, new Rect(-1.5, -1.5, selectionBounds.Width, selectionBounds.Height), selectionRadiusX, selectionRadiusY);
                        }
                    }
                    break;
                case ShapeType.Star:
                    drawingContext.DrawGeometry(Shape.Fill, pen, GetStarGeometry(Shape.Bounds, new Vector(0, 0)));
                    if (Selected)
                    {
                        drawingContext.DrawGeometry(null, selectionPen, GetStarGeometry(selectionBounds, new Vector(-1.5, -1.5)));
                    }
                    break;
                case ShapeType.CustomDrawableObject:
                    Shape.OnRender(drawingContext, scale, Selected);
                    break;
            }
        }

        private Geometry GetStarGeometry(Rect bounds, Vector offset)
        {
            double a = bounds.Width / 2;
            double b = bounds.Height / 2;
            double c = bounds.Width / 10;
            double d = bounds.Height / 10;
            Point center = new Point(a, b) + offset;

            PathGeometry g = new PathGeometry();
            PathFigure f = new PathFigure() { StartPoint = center + PointOnEllipse(0, a, b) };
            f.IsClosed = true;
            double step = 360 / (2 * Shape.StarPoints);
            for (double angle = 0; angle < 360 - step; angle += step * 2)
            {
                var outer = PointOnEllipse(angle, a, b) + center;
                var inner = PointOnEllipse(angle + step, c, d) + center;
                f.Segments.Add(new LineSegment(outer, true));
                f.Segments.Add(new LineSegment(inner, true));
            }
            g.Figures.Add(f);
            return g;
        }


        private Vector PointOnEllipse(double angle, double a, double b)
        {
            double d = angle * Math.PI / 180;
            return new Vector(a * Math.Cos(d), b * Math.Sin(d));
        }

        public void OnZoomChange(double newZoomLevel)
        {
            //   this.scale = newZoomLevel == 0 ? 0.000001 : newZoomLevel;
            if (Shape.Stroke != null)
            {
                InvalidateVisual();
            }
        }

    }
}
