using System.Windows;
using System.Windows.Media;

namespace Prototype.Main
{
    public interface IUIHelper
    {
        void Initialize(FrameworkElement element);
        double PixelsPerDip { get; }
    }

    public class UIHelper : IUIHelper
    {
        public UIHelper() 
        {
        }

        private FrameworkElement? _element = null;
        public void Initialize(FrameworkElement element)
        {
            _element = element;
        }

        public double PixelsPerDip
        {
            get
            {
                if (_element == null) 
                {
                    throw new InvalidOperationException("UIHelper is not initalized!");
                }
                return VisualTreeHelper.GetDpi(_element).PixelsPerDip;
            }
         }
    }
}
