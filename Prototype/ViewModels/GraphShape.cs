using Prototype.Main.VirtualCanvas;
using Prototype.Main.VirtualCanvas.Interfaces;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Main.ViewModels
{
    public class GraphShape : DemoShape
    {
        record HeaderInfo(Point Point, Object Object);

        private readonly IUIHelper _uIHelper;

        public ProjectViewModel Project { get; private set; }

        public GraphShape(double x, double y, ProjectViewModel detail, IUIHelper uIHelper)
        {
            _uIHelper = uIHelper;
            Bounds = new Rect(new Point(x, y), Calculate(detail));
            Project = detail;
            CanZoom = false;
            CanMove = true;
        }

        public static bool IsMatched(ISpatialItem item, object detail) 
        {
            return ReferenceEquals(((GraphShape)item)?.Project, detail); 
        }

        public Point? GetByParent(object parent)
        {
            double height = 0;
            height += 2;
            var pixelsPerDip = _uIHelper.PixelsPerDip;
            var headers = CalculateHeaders(Project, pixelsPerDip);
            height += headers[0].Height + 2;
            var text = Project.Info;
            height += headers[1].Height + 2;
            foreach (var item in Project.FlatList)
            {
                if (item is DependedProjectViewModel)
                {
                    if (item == parent)
                    {
                        return new Point(25 + 5 + Bounds.X, Bounds.Y + height);
                    }
                    headers = CalculateHeaders((DependedProjectViewModel)item, pixelsPerDip);
                    height += headers[0].Height + 2;
                }
                else if (item is ClassViewModel)
                {
                    headers = CalculateHeaders((ClassViewModel)item, pixelsPerDip);
                    height += headers[0].Height + 2;
                }
            }
            return null;
        }

        public Point GetHeader()
        {
            return new Point(Header.X + Bounds.X, Bounds.Y + Header.Y);
        }

        private Point Header => new Point(5, 2);

        public FormattedText CalculateHeader() => CalculateHeaders(Project, _uIHelper.PixelsPerDip)[0];

        public FormattedText[] CalculateHeaders(DependedProjectViewModel parent) => CalculateHeaders(parent, _uIHelper.PixelsPerDip);

        private static FormattedText[] CalculateHeaders(ProjectViewModel detail, double pixelsPerDip)
        {
            FormattedText[] result = new FormattedText[2];
            var text = detail.Description;
            result[0] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  12, System.Windows.Media.Brushes.Black, pixelsPerDip);
            text = detail.Info;
            result[1] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  10, System.Windows.Media.Brushes.Gray, pixelsPerDip);
            return result;
        }

        private static FormattedText[] CalculateHeaders(DependedProjectViewModel parent, double pixelsPerDip)
        {
            FormattedText[] result = new FormattedText[2];
            var text = parent.Description;
            result[0] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  8, System.Windows.Media.Brushes.Black, pixelsPerDip);
            text = parent.Info;
            result[1] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  6, System.Windows.Media.Brushes.Black, pixelsPerDip);
            return result;
        }

        private static FormattedText[] CalculateHeaders(ClassViewModel parent, double pixelsPerDip)
        {
            FormattedText[] result = new FormattedText[2];
            var text = parent.Description;
            result[0] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  6, System.Windows.Media.Brushes.Black, pixelsPerDip);
            text = parent.Info.ToString();
            result[1] = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"),
                  6, System.Windows.Media.Brushes.Gray, pixelsPerDip);
            return result;
        }

        private Size Calculate(ProjectViewModel detail)
        {
            double width = 0;
            double height = 0;
            height += 2;
            var pixelsPerDip = _uIHelper.PixelsPerDip;
            var headers = CalculateHeaders(detail, pixelsPerDip);
            width += 20 + headers[0].Width;
            height += headers[0].Height + 2;
            var text = detail.Info;
            width = Math.Max(width, headers[1].Width + 20);
            height += headers[1].Height + 2;
            foreach (var item in detail.FlatList)
            {
                if (item is DependedProjectViewModel)
                {
                    headers = CalculateHeaders((DependedProjectViewModel)item, pixelsPerDip);
                    width = Math.Max(width, headers[0].Width + 27);
                    height += headers[0].Height + 2;
                }
                else if (item is ClassViewModel)
                {
                    headers = CalculateHeaders((ClassViewModel)item, pixelsPerDip);
                    width = Math.Max(width, headers[0].Width + 32);
                    height += headers[0].Height + 2;
                }
                else Debug.Assert(false, "Unknown type");
            }
            return new Size(width, height);
        }


        public override void OnRender(DrawingContext drawingContext, double scale, bool selected)
        {
            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(Stroke, StrokeThickness / scale);
            drawingContext.DrawRectangle(Fill, pen, new Rect(0, 0, Bounds.Width, Bounds.Height));
            var pixelsPerDip = _uIHelper.PixelsPerDip;
            var headers = CalculateHeaders(Project, pixelsPerDip);
            drawingContext.DrawText(headers[0], Header);
            double x = 5;
            double y = headers[0].Height + 4;
            drawingContext.DrawText(headers[1], new System.Windows.Point(x, y));
            x = 5;
            y += headers[1].Height + 2;
            foreach (var item in Project.FlatList)
            {
                if (item is DependedProjectViewModel)
                {
                    x = 5 + 20;
                    headers = CalculateHeaders((DependedProjectViewModel)item, pixelsPerDip);
                    drawingContext.DrawText(headers[0], new System.Windows.Point(x, y));
                    x += headers[0].Width + 2;
                    y += headers[0].Height + 2;
                }
                else if (item is ClassViewModel)
                {
                    x = 5 + 30;
                    headers = CalculateHeaders((ClassViewModel)item, pixelsPerDip);
                    drawingContext.DrawText(headers[0], new System.Windows.Point(x, y));
                    x += headers[0].Width + 2;
                    y += headers[0].Height + 2;
                }
                else Debug.Assert(false, "Unknown type");
            }
            if (selected)
            {
                var selectionBounds = Bounds;
                selectionBounds.Inflate(1.5, 1.5);
                var selectionPen = new System.Windows.Media.Pen(Brushes.Yellow, 3 / scale);
                drawingContext.DrawRectangle(null, selectionPen, new Rect(-1.5, -1.5, selectionBounds.Width, selectionBounds.Height));
            }
        }
    }
}
