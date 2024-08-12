using Prototype.Main.VirtualCanvas;
using Prototype.Main.VirtualCanvas.Interfaces;
using System.Windows;
using System.Windows.Controls;
namespace Prototype.Main.Controls
{
    public class VirtualCanvasScrollViewer : ScrollViewer
    {
        private DemoDiagram? _canvas;

        protected class InternalMediator : ICanvasProxy
        {
            private readonly DemoDiagram _canvas;

            public InternalMediator(DemoDiagram canvas)
            {
                _canvas = canvas;
            }

            public void Fill(DemoSpatialIndex spatialIndex)
            {
                _canvas.Index = spatialIndex;
                _canvas.ScrollExtent = spatialIndex.Extent;
            }

            public void MoveScrollTo(Point point, bool animate)
            {
                _canvas.MoveTo(point, animate);
            }

            public IEnumerable<T> Generate<T>()
            {
                return _canvas.Index.OfType<ISpatialItem>().OfType<T>();
            }

            public bool Remove(ISpatialItem item)
            {
                return _canvas.Index.Remove(item);
            }

            public void Add(DemoShape shape)
            {
                _canvas.Index.Insert(shape);
            }
        }

        public static readonly DependencyProperty ProxyOwnerProperty = DependencyProperty.Register("ProxyOwner", typeof(object), typeof(VirtualCanvasScrollViewer), new PropertyMetadata(null, OnOwnerPropertyChanged));
        public object ProxyOwner
        {
            get { return this.GetValue(ProxyOwnerProperty); }
            set
            {
                this.SetValue(ProxyOwnerProperty, value);
            }
        }

        private static void OnOwnerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as VirtualCanvasScrollViewer;
            if (control != null) 
            {
                var oldProxyOwner = e.OldValue as ICanvasProxyOwner;
                if (oldProxyOwner != null)
                    oldProxyOwner.CanvasProxy = null;

                var newProxyOwner = e.NewValue as ICanvasProxyOwner;
                if (newProxyOwner != null && control._canvas != null)
                    newProxyOwner.CanvasProxy = new InternalMediator(control._canvas!);
            }
        }

        public VirtualCanvasScrollViewer()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _canvas = (DemoDiagram)Content;
            if (!(_canvas is DemoDiagram))
            {
                throw new Exception("VirtualCanvasProxy requires DemoDiagram as content");
            }
            var proxy = ProxyOwner as ICanvasProxyOwner;
            if (proxy != null)
                proxy.CanvasProxy = new InternalMediator(_canvas);
        }
    }
}
