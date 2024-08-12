using BotTimeICSharpCode.AvalonEdit.Document;
using BotTimeICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Main.Controls
{
    public class CustomTextView : TextView
    {
        public CustomTextView()
        {
            var highlightedTextColorizer = new HighlightedTextColorizer(this);
            LineTransformers.Insert(0, highlightedTextColorizer);
        }

        public static readonly DependencyProperty SearchTextOwnerProperty = DependencyProperty.Register("SearchTextOwner",
       typeof(ISearchTextOwner), typeof(CustomTextView), new PropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }

        public static readonly DependencyProperty HighlightedTextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CustomTextView),
            new PropertyMetadata(string.Empty, UpdateControlCallBack));
        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customTextView = d as CustomTextView;
            if (customTextView != null && e.NewValue != e.OldValue)
                customTextView.Document = new TextDocument() { Text = (string)e.NewValue };
        }


        public ISearchTextOwner SearchTextOwner
        {
            get { return (ISearchTextOwner)GetValue(SearchTextOwnerProperty); }
            set { SetValue(SearchTextOwnerProperty, value); }
        }
    }

    internal class HighlightedTextColorizer : DocumentColorizingTransformer
    {
        public HighlightedTextColorizer(CustomTextView parent)
        {
            _parent = parent;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            var text = _parent?.SearchTextOwner?.SearchText ?? "";
            if (string.IsNullOrEmpty(text))
                return;

            var separator = System.IO.Path.GetInvalidPathChars().Take(4).ToString()!;

            var formatedStringInfos = _parent!.Text.Replace(text, separator + text + separator).
                                   Split(new string[] { separator }, StringSplitOptions.None).
                                   Where(it => !string.IsNullOrEmpty(it)).
                                   Select(it =>
                                   new { Text = it, IsHightLight = it == text }).ToList();

            var currentLineStartOffset = line.Offset;

            foreach (var formatedStringInfo in formatedStringInfos)
            {
                var currentLineEndOffset = currentLineStartOffset + formatedStringInfo.Text.Length;
                if (formatedStringInfo.IsHightLight)
                    ChangeLinePart(currentLineStartOffset, currentLineEndOffset, (lineElement) => ColorizeDiffLine(lineElement));
                currentLineStartOffset = currentLineEndOffset;
            }
        }

        private void ColorizeDiffLine(VisualLineElement lineElement)
        {
            Typeface tf = lineElement.TextRunProperties.Typeface;
            lineElement.TextRunProperties.SetTypeface(new Typeface(
                    tf.FontFamily, tf.Style, FontWeights.Bold, tf.Stretch));
        }

        private readonly CustomTextView _parent;
    }
}
