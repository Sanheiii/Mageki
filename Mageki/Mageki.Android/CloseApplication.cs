
using Mageki.DependencyServices;
using Mageki.Droid;

using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplication))]
namespace Mageki.Droid
{
    public class CloseApplication : ICloseApplication
    {
        public void Close()
        {
            Xamarin.Essentials.Platform.CurrentActivity.Finish();
        }
    }
}