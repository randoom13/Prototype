
using Prototype.Main.VirtualCanvas;
using Prototype.Main.VirtualCanvas.Interfaces;
using System.Windows;

namespace Prototype.Main.Controls
{
    internal interface ICanvasProxy
    {
        void MoveScrollTo(Point point, bool animate);
        void Fill(DemoSpatialIndex spatialIndex);
        IEnumerable<T> Generate<T>();
        bool Remove(ISpatialItem item);
        void Add(DemoShape shape);
    }

    internal interface ICanvasProxyOwner
    {
        ICanvasProxy? CanvasProxy { get; set; }
    }
}
