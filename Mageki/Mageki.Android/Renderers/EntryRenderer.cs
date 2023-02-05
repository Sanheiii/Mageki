using Android.Content;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(Mageki.Droid.Renderers.EntryRenderer))]
namespace Mageki.Droid.Renderers
{
    public class EntryRenderer : Xamarin.Forms.Platform.Android.EntryRenderer
    {
        public EntryRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Control.ViewAttachedToWindow += Control_ViewAttachedToWindow;
        }

        private void Control_ViewAttachedToWindow(object sender, ViewAttachedToWindowEventArgs e)
        {
            //Control.Background = null;
            Control.Enabled = !Control.Enabled;
            Control.Enabled = !Control.Enabled;
        }
    }
}