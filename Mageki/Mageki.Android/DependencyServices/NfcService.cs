
using Mageki.DependencyServices;
using Mageki.Droid.DependencyServices;

using System;

using Xamarin.Forms;

[assembly: Dependency(typeof(NfcService))]
namespace Mageki.Droid.DependencyServices
{
    public class NfcService : INfcService
    {
        public bool ReadingAvailable => true;

        public Action<byte[]> OnScanAction;
        public Action OnInvalidate;

        public void StartReadFelicaId(Action<byte[]> onScanAction, Action onInvalidate)
        {
            OnScanAction = onScanAction;
            OnInvalidate = onInvalidate;
        }
    }
}