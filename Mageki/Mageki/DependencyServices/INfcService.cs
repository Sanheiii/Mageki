using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.DependencyServices
{
    public interface INfcService
    {
        public bool ReadingAvailable { get; }
        public void StartReadFelicaId(Action<byte[]> onScanAction, Action onInvalidate);
    }
}
