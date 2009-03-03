using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using doru;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Notepad
{
    public class MyTextBox : TextBox
    {

        string _Find;
        public void Find(string Find)
        {
            _Find = Find;
            FindNext();
        }
        public void FindNext()
        {
            Focus();
            Match m = new Regex(_Find,RegexOptions.IgnoreCase| RegexOptions.Multiline).Match(Text, SelectionStart + SelectionLength);
            if (m.Success)
            {
                SelectionStart = m.Index;
                SelectionLength = m.Length;
            }
            else
            {
                SelectionStart = 0;
                SelectionLength = 0;
            }
            
        }
    }
    public class MyRitchText : RichTextBox
    {
        public MyRitchText()
        {            
        }

        
        public TextRange _TextRange
        {
            get { return new TextRange(Document.ContentStart, Document.ContentEnd); }
            set { _Text = value.Text; }
        }
        //public static readonly DependencyProperty _TextRangeProperty = DependencyProperty.Register("_TextRange", typeof(TextRange), typeof(MyRitchText), new PropertyMetadata(Helper.OnPropertyChangedCallback));

        public string _Text
        {
            get { return _TextRange.Text; }
            set
            {                
                _TextRange.Text = value;
            }
        }
        public static readonly DependencyProperty _TextProperty = DependencyProperty.Register("_Text", typeof(string), typeof(MyRitchText), new PropertyMetadata(Helper.OnPropertyChangedCallback));

        List<TextRange> _trs = new List<TextRange>();

        public void Find(string _find)
        {
            TextRange documentRange = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            documentRange.ClearAllProperties();
            _trs.Clear();
            TextPointer navigator = this.Document.ContentStart;
            while (navigator.CompareTo(this.Document.ContentEnd) < 0)
            {
                TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    Run r = ((Run)navigator.Parent);
                    string text = r.Text;

                    Match m = Regex.Match(text, _find);
                    if (m.Success)
                    {
                        TextPointer tp = r.ElementStart;
                        TextRange range = new TextRange(tp.GetPositionAtOffset(m.Index + 1), tp.GetPositionAtOffset(m.Index + m.Length + 1));
                        _trs.Add(range);
                    }
                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }
            foreach (TextRange range in _trs)
                range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            FindNext();

        }

        public void FindNext()
        {
            if (_trs.Count == 0) return;
            foreach (TextRange t in _trs)
            {
                int c = t.Start.CompareTo(this.Selection.End);
                if (c > 0)
                {
                    Select(t);
                    return;
                }
            }
            Select(_trs.First());
        }

        private void Select(TextRange t)
        {
            this.Focus();
            this.Selection.Select(t.Start, t.End);
        }
    }
    

}
