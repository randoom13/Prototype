using Prototype.Main.VirtualCanvas;
using Prototype.Main.VirtualCanvas.Interfaces;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Prototype.Main.ViewModels
{
    public class GraphArrowLineWithInfo : DemoShape
    {
        private readonly GraphShape _beginPointShape;
        private readonly GraphShape _endPointShape;
        private readonly DependedProjectViewModel _beginParent;

        public void Detach()
        {
            _beginPointShape.PropertyChanged -= OnShapePropertyChanged;
            _endPointShape.PropertyChanged -= OnShapePropertyChanged;
            _beginPointShape.Project.FlatList.CollectionChanged -= OnBeginProjectFlatListChanged;
            Bounds = new Rect(0,0,0,0);
        }

        private void OnBeginProjectFlatListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLines();
        }

        private void OnShapePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISpatialItem.Bounds))
                RefreshLines();
        }

        private void RefreshLines()
        {
            RefreshLines((Point)_beginPointShape.GetByParent(_beginParent)!);
        }

        private GraphArrowLineWithInfo(GraphShape beginShape, 
            GraphShape endShape, DependedProjectViewModel beginParent, Point begin)
        {
            ZIndex = 2;
            CanZoom = false;
            CanMove = false;
            _beginPointShape = beginShape;
            _endPointShape = endShape;
            _beginParent = beginParent;
            RefreshLines(begin);
            Type = ShapeType.CustomDrawableObject;
            Priority = 1;
            _beginPointShape.PropertyChanged += OnShapePropertyChanged;
            _beginPointShape.Project.FlatList.CollectionChanged += OnBeginProjectFlatListChanged;
            _endPointShape.PropertyChanged += OnShapePropertyChanged;
        }

        private static GeometryGroup DrawLinkArrow(Point p1, Point p2)
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            pathFigure.StartPoint = p;
            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);
            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);
            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);
            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);
            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            return lineGroup;
        }

        private void RefreshLines(Point begin)
        {
            var end = _endPointShape!.GetHeader();
            var rect = new Rect(begin, end);
            _lines = new Point[3];
            _lines[0] = new Point(begin.X - rect.X, begin.Y - rect.Y);
            _lines[1] = new Point(end.X - rect.X, end.Y - rect.Y);
            var horizontalPercentage = _random.Next(30) + 50;
            var verticalpercentage = _random.Next(33) + 50;
            _lines[2] = new Point(_lines[1].X*horizontalPercentage/100, _lines[1].Y * verticalpercentage / 100);
            Bounds = rect;
        }

        public bool IsBeginFrom(object detail)
        {
            return ReferenceEquals(_beginPointShape.Project, detail);
        }

        public static GraphArrowLineWithInfo? BuildGraphLine(GraphShape beginShape, GraphShape endShape, DependedProjectViewModel beginParent)
        {
            var begin = beginShape.GetByParent(beginParent);
            if (begin == null)
            {
                Debug.Assert(false);
                return null;
            }
            return new GraphArrowLineWithInfo(beginShape, endShape, beginParent, (Point)begin);
        }

        private Point[] _lines = new Point[2];

        private static Random _random = new Random(Environment.TickCount);

        public override void OnRender(DrawingContext drawingContext, double scale, bool selected)
        {
            if (Bounds.Width == 0 && Bounds.Height == 0)
                return;

            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(Stroke, StrokeThickness / scale);
            if (_lines[0].X == _lines[1].X || _lines[0].Y == _lines[1].Y)
            {
                drawingContext.DrawGeometry(null, pen, DrawLinkArrow(_lines[0], _lines[1]));
            }
            else
            {
                var point1 = new Point(_lines[2].X, _lines[0].Y);
                drawingContext.DrawLine(pen, _lines[0], point1);
                var point2 = new Point(point1.X, _lines[2].Y);
                drawingContext.DrawLine(pen, point1, point2);
                var point3 = new Point(_lines[1].X, point2.Y);
                drawingContext.DrawLine(pen, point2, point3);
                drawingContext.DrawGeometry(null, pen, DrawLinkArrow(point3, _lines[1]));
                var formatedText = _beginPointShape.CalculateHeaders(_beginParent)[1]!;
                var white = new System.Windows.Media.SolidColorBrush(Colors.White);
                var textPoint = new Point((point1.X + _lines[0].X) / 2, _lines[0].Y - formatedText.Height / 2);
                drawingContext.DrawRectangle(white,
                    new System.Windows.Media.Pen(white, StrokeThickness / scale),
                    new Rect(textPoint.X, textPoint.Y, formatedText.Width, formatedText.Height));
                drawingContext.DrawText(formatedText, textPoint);
            }
;            if (selected)
            {
                var selectionBounds = Bounds;
                selectionBounds.Inflate(1.5, 1.5);
                var selectionPen = new System.Windows.Media.Pen(Brushes.Yellow, 3 / scale);
                drawingContext.DrawLine(selectionPen, _lines[0], _lines[1]);
            }

        }
    }
}
