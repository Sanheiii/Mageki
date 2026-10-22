namespace Mageki.Services;

public interface INfcService
{
    bool ReadingAvailable { get; }

    void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate);
}
