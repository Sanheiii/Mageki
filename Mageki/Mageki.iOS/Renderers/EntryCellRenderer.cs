using Foundation;

using Mageki.iOS.Renderers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Mageki.EntryCell), typeof(Mageki.iOS.Renderers.EntryCellRenderer))]
namespace Mageki.iOS.Renderers
{
    public class EntryCellRenderer : Xamarin.Forms.Platform.iOS.EntryCellRenderer
    {
        Mageki.EntryCell element;
        UITextField text;
        private readonly Color _defaultPlaceholderColor =Color.Gray;
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            element = (EntryCell)item;
            var cell = base.GetCell(element, reusableCell, tv);
            text = ((UITextField)cell.Subviews[0].Subviews[0]);
            text.TextColor = element.TextColor.ToUIColor();
            return cell;
        }
        protected virtual void UpdateAttributedPlaceholder(NSAttributedString nsAttributedString)
        {
            text.AttributedPlaceholder = nsAttributedString;
        }
    }
}