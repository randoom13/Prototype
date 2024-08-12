//-----------------------------------------------------------------------
// <copyright file="DefaultVisualFactory.cs" company="Microsoft">
//   (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Prototype.Main.VirtualCanvas.Controls
{
    /// <summary>
    /// This factory creates visuals by finding and loading DataTemplates that match the actual type of the
    /// ISpatialItems.  If no template is found it creates a TextBlock
    /// </summary>
    public class DefaultVisualFactory : TemplatedVisualFactory
    {
        private static readonly ComponentResourceKey FallbackTemplateKey = new ComponentResourceKey(typeof(DefaultVisualFactory), "DataTemplateFactoryFallbackTemplate");

        /// <summary>
        /// Helper to allow easy choosing of templates.
        /// </summary>
        private RealizationHelper realizationHelper;

        /// <summary>
        /// Construct an instance of the factory
        /// </summary>
        /// <param name="addVisualChildMethod">This parameter is a method to add an item to the visual tree from which to resolve data templates.</param>
        /// <remarks>Any element added via the addVisualChildMethod delegate should not have any impact on visual rendering</remarks>
        public DefaultVisualFactory(Action<Visual>? addVisualChildMethod)
        {
            if (addVisualChildMethod == null)
            {
                throw new ArgumentNullException("addVisualChildMethod");
            }
            // Use a hidden helper added to the visual tree to locate templates.
            realizationHelper = new RealizationHelper();
            addVisualChildMethod(realizationHelper);
        }


        private DataTemplate? fallbackTemplate;

        /// <summary>
        /// Template to use if there is none found for the ISpatialItem
        /// </summary>
        public virtual DataTemplate? FallbackTemplate
        {
            get
            {
                try
                {
                    if (fallbackTemplate == null)
                    {
                        fallbackTemplate = realizationHelper.FindResource(FallbackTemplateKey) as DataTemplate;
                    }

                    return fallbackTemplate;
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    return null;
                }
            }
        }


        #region Extension Points

        /// <summary>
        /// Choose a template for the given item.
        /// </summary>
        /// <param name="item">the item to choose a template for</param>
        /// <returns>A template for the given item or null if one cannot be located</returns>
        protected override DataTemplate? ChooseTemplate(object? item)
        {
            if (item == null)
            {
                return null;
            }
            return realizationHelper.ChooseTemplate(item);
        }

        protected override Visual? ProduceDefaultVisual(object? item)
        {
            if (item == null)
            {
                return null;
            }

            Visual? defaultVisual = null;
            if (FallbackTemplate != null)
            {
                defaultVisual = ProduceVisual(item, FallbackTemplate);
            }
            if (defaultVisual == null)
            {
                return base.ProduceDefaultVisual(item);
            }
            else
            {
                IAddChild? textHost = defaultVisual as IAddChild;
                if (textHost != null)
                {
                    try
                    {
                        textHost.AddText(string.Format(CultureInfo.CurrentCulture, "DataTemplateNotFound: {0}", item.GetType().Name));
                    }
                    catch (ArgumentException)
                    {
                        // We can cope with failure to add text.
                    }
                }
            }

            return defaultVisual;
        }
        #endregion

        /// <summary>
        /// A simplified content presenter to help choose templates
        /// </summary>
        private class RealizationHelper : ContentPresenter
        {

            public RealizationHelper() : base()
            {
                Visibility = Visibility.Hidden;
            }

            public DataTemplate? ChooseTemplate(object? item)
            {
                DataTemplate? template = null;
                try
                {
                    Content = item;
                    template = ChooseTemplate();
                }
                finally
                {
                    Content = null;
                }
                return template;
            }
        }
    }
}
