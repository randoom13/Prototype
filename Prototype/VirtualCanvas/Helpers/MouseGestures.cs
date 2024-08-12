using System;
using System.Windows;
using System.Windows.Media;
using Prototype.Main.VirtualCanvas;

namespace Prototype.Main.VirtualCanvas.Helpers
{
    internal class MouseGestures
    {
        private bool isMouseDown;
        private bool started;
        private bool captured;
        private Point previousPosition;
        private DemoDiagram owner;
        private DemoShapeVisual? hit;

        /// <summary>
        /// Threashold used to know if we can start the movement of the selection or not. We start the movement when the distance
        /// between the previousPosition and the current mouse position is more than the threshold
        /// </summary>
        public const double Threshold = 2;

        public MouseGestures(DemoDiagram owner)
        {
            this.owner = owner;
            owner.PreviewMouseDown += OnMouseDown;
            owner.PreviewMouseMove += OnMouseMove;
            owner.PreviewMouseUp += OnMouseUp;
            owner.LostMouseCapture += OnLostMouseCapture;
        }

        private void OnLostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FinishPanning();
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FinishPanning();
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            owner.Focus();
            isMouseDown = true;
            started = false;
            previousPosition = e.GetPosition(owner);
            hit = HitTestVisual(previousPosition);
            if (hit != null)
            {
                var shape = hit.Shape
;                if (!shape.CanZoom && !shape.CanMove)
                {
                    owner.Selection = null;
                    return;
                }
                if (shape.CanZoom)
                {
                    var bounds = hit.Shape.Bounds;
                    bounds.Inflate(3, 3);
                    hit.Shape.Bounds = bounds;
                }
            }
            else
            {
                owner.Selection = null;
            }
            captured = owner.CaptureMouse();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                var position = e.GetPosition(owner);
                if (!started)
                {
                    Vector deplacement = position - previousPosition;
                    if (deplacement.Length > Threshold)
                    {
                        started = true;
                    }
                }
                else
                {
                    // request to move only if needed, in order to save a few CPU cylces.
                    if (position != previousPosition)
                    {
                        double dx = position.X - previousPosition.X;
                        double dy = position.Y - previousPosition.Y;
                        if (hit != null && hit.Selected)
                        {
                            if (hit.Shape.CanZoom)
                            {
                                // then move the shape
                                var bounds = hit.Shape.Bounds;
                                bounds.Offset(dx / owner.Scale, dy / owner.Scale);
                                hit.Shape.Bounds = bounds;
                            }
                            else if (hit.Shape.CanMove) 
                            {
                                // then move the shape
                                var bounds = hit.Shape.Bounds;
                                bounds.Offset(dx, dy);
                                hit.Shape.Bounds = bounds;
                            }
                        }
                        else
                        {
                            // then pan the view.
                            owner.MoveBy(dx, dy);
                        }
                        previousPosition = position;
                    }
                }
            }
        }

        private void FinishPanning()
        {
            if (hit != null && hit.Shape.CanZoom)
            {
                var bounds = hit.Shape.Bounds;
                bounds.Inflate(-3, -3);
                hit.Shape.Bounds = bounds;
            }
            if (!started)
            {
                if (hit != null)
                {
                    owner.Selection = hit;
                }
            }
            isMouseDown = false;
            if (captured)
            {
                captured = false;
                owner.ReleaseMouseCapture();
            }
        }

        public DemoShapeVisual? HitTestVisual(Point hitPoint)
        {
            DemoShapeVisual? found = null;
            try
            {
                // Hit test radius in screen distance needs to be increased when zoomed out.
                double radius = 5 / Math.Max(owner.Scale, 1);

                // Expand the hit test area by creating a geometry centered on the hit test point.
                var expandedHitTestArea = new GeometryHitTestParameters(new EllipseGeometry(hitPoint, radius, radius));

                // The following callback is called for every visual intersecting with the test area, in reverse z-index order.
                // If hitLinks is true, all links passing through the test area are considered, and the closest to the hitPoint is returned in visual.
                // The hit test search is stopped as soon as the first non-link graph object is encountered.
                var hitTestResultCallback = new HitTestResultCallback(r =>
                {
                    if (r.VisualHit is DemoShapeVisual v)
                    {
                        found = v;
                        return HitTestResultBehavior.Stop;
                    }
                    return HitTestResultBehavior.Continue;
                });

                // start the search
                VisualTreeHelper.HitTest(owner, null, hitTestResultCallback, expandedHitTestArea);
            }
            catch (Exception)
            {
                // silently swallow the exception because if we can't find any child node, then we can proceed
                // by just returning null.  The control will probably correct its state on next graph layout.
            }
            return found;
        }

    }
}
