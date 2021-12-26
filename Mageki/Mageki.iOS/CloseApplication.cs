
using Mageki.DependencyServices;
using Mageki.iOS;

using System.Threading;

using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplication))]
namespace Mageki.iOS
{
    public class CloseApplication : ICloseApplication
    {
        public void Close()
        {
            Thread.CurrentThread.Abort();
        }
    }
}