
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

        public Action<byte[]> OnFelicaScan;
        public Action<byte[]> OnMifareScan;
        public Action OnInvalidate;

        public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate)
        {
            OnFelicaScan = onFelicaScan;
            OnMifareScan = onMifareScan;
            OnInvalidate = onInvalidate;
        }
    }
}