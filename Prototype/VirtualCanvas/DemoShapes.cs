using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Prototype.Main.VirtualCanvas.Interfaces;

namespace Prototype.Main.VirtualCanvas
{
    public enum ShapeType
    {
        Ellipse,
        Rect,
        RoundedRect,
        Star,
        CustomDrawableObject
    }

    /// <summary>
    /// A light weight "data" object that contains the bare minimum info needed to 
    /// create the virtualized visuals in the DemoShapeFactory.
    /// </summary>
    public class DemoShape : ISpatialItem, INotifyPropertyChanged
    {
        private Rect bounds;
        private double priority;
        private int zindex;
        private bool visible;
        private bool selected;
        private Brush fill;
        private Brush stroke;
        private double strokeThickness;
        private int starPoints;
        private ShapeType shapeType;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DemoShape()
        {
            DataItem = this;
        }

        /// <summary>
        /// The bounds of the item.
        /// </summary>
        public Rect Bounds
        {
            get => bounds;
            set
            {
                if (bounds != value)
                {
                    bounds = value;
                    OnChanged("Bounds");
                }
            }
        }

        public bool CanZoom { get; protected set; } = true;
        public bool CanMove { get; protected set; } = false;

        /// <summary>
        /// A value indicating how visually important the element is relative to other items.
        /// Higher priorities will be displayed first.
        /// </summary>
        public double Priority
        {
            get => priority;
            set
            {
                if (priority != value)
                {
                    priority = value;
                    OnChanged("Priority");
                }
            }
        }

        /// <summary>
        /// The Z-Index of the visual relative to other items.
        /// Higher ZIndexs will be drawn on top.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ZIndex")]
        public int ZIndex
        {
            get => zindex;
            set
            {
                if (zindex != value)
                {
                    zindex = value;
                    OnChanged("ZIndex");
                }
            }
        }


        /// <summary>
        /// Invoked when Visual whose DataContext is this 
        /// SpatialItem is measured.
        /// </summary>
        /// <param name="visual">The Visual corresponding to the ISpatialItem</param>
        public virtual void OnMeasure(UIElement visual)
        {

        }

        /// <summary>
        /// Return a user defined data item associated with this object.
        /// </summary>
        public object DataItem { get; set; }

        public virtual void OnRender(DrawingContext drawingContext, double scale, bool Selected)
        {

        }
        /// <summary>
        /// Determines whether the item is visible on the canvas.
        /// </summary>
        public bool IsVisible
        {
            get => visible; set
            {
                if (visible != value)
                {
                    visible = value;
                    OnChanged("IsVisible");
                }
            }
        }


        public bool IsSelected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    OnChanged("IsSelected");
                }
            }
        }


        public Brush Fill
        {
            get => fill;
            set
            {
                if (fill != value)
                {
                    fill = value;
                    OnChanged("Fill");
                }
            }
        }

        public Brush Stroke
        {
            get => stroke;
            set
            {
                if (stroke != value)
                {
                    stroke = value;
                    OnChanged("Stroke");
                }
            }
        }
        public double StrokeThickness
        {
            get => strokeThickness;
            set
            {
                if (strokeThickness != value)
                {
                    strokeThickness = value;
                    OnChanged("StrokeThickness");
                }
            }
        }

        public ShapeType Type
        {
            get => shapeType;
            set
            {
                if (shapeType != value)
                {
                    shapeType = value;
                    OnChanged("Type");
                }
            }
        }

        public int StarPoints
        {
            get => starPoints;
            set
            {
                if (starPoints != value)
                {
                    starPoints = value;
                    OnChanged("        public int StarPoints\r\n");
                }
            }
        }

        private void OnChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
