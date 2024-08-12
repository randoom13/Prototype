using System.Windows;
using System.Windows.Controls;

namespace Prototype.Main.Controls
{
    public class TypeBasedTemplateSelector : DataTemplateSelector
    {
        private readonly Dictionary<string, DataTemplate> _templates = new Dictionary<string, DataTemplate>();
        
        public Dictionary<string, DataTemplate> Templates => _templates;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return base.SelectTemplate(item, container);

            var key = item.GetType().Name;
            DataTemplate? template = null;

            if (!Templates.TryGetValue(key, out template))
                return base.SelectTemplate(item, container);

            return template;
        }
    }
}
