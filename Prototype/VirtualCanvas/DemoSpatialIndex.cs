using Prototype.Main.VirtualCanvas.Interfaces;
using Prototype.Main.VirtualCanvas.QuadTree;
using System.ComponentModel;

namespace Prototype.Main.VirtualCanvas
{
    /// <summary>
    /// This provides the ISpatialIndex needed by VirtualCanvas, implemented using our
    /// handy PriorityQuadTree helper class.
    /// </summary>
    public class DemoSpatialIndex : PriorityQuadTree<ISpatialItem>, ISpatialIndex
    {
        public event EventHandler? Changed;

        public void Insert(DemoShape item)
        {
            item.PropertyChanged -= OnItemChanged; // make sure we never add this handler twice
            item.PropertyChanged += OnItemChanged;

            Insert(item, item.Bounds, 0);

            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DemoShape shape)
            {
                if (e.PropertyName == "Bounds")
                {
                    // needs to be reindexed.
                    Remove(shape);
                    Insert(shape);
                }
            }
        }
    }
}
