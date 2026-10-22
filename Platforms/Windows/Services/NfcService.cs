using Mageki.Services;

namespace Mageki.Platforms.Windows.Services;

public sealed class NfcService : INfcService
{
    public bool ReadingAvailable => false;

    public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate)
    {
        onInvalidate();
    }
}
