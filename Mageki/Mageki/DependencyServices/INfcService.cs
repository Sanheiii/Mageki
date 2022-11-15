using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.DependencyServices
{
    public interface INfcService
    {
        public bool ReadingAvailable { get; }
        public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate);
    }
}
