using Prototype.Main.VirtualCanvas.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Main.VirtualCanvas
{
    class DemoShapeFactory : IVisualFactory
    {
        public void BeginRealize()
        {
        }

        public void EndRealize()
        {
        }

        public Visual? Realize(object? item, bool force)
        {
            if (item is DemoShape d)
            {
                return new DemoShapeVisual()
                {
                    Shape = d
                };
            }
            return null;
        }


        private Point PointOnEllipse(double angle, double a, double b)
        {
            double d = angle * Math.PI / 180;
            return new Point(a * Math.Cos(d), b * Math.Sin(d));
        }

        public bool Virtualize(Visual visual)
        {
            return true;
        }

    }
}
