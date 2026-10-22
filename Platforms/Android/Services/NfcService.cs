using Mageki.Services;

namespace Mageki.Platforms.Android.Services;

public sealed class NfcService : INfcService
{
    public bool ReadingAvailable =>
        global::Android.Nfc.NfcAdapter.GetDefaultAdapter(global::Android.App.Application.Context)?.IsEnabled == true;

    public Action<byte[]>? OnFelicaScan { get; private set; }

    public Action<byte[]>? OnMifareScan { get; private set; }

    public Action? OnInvalidate { get; private set; }

    public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate)
    {
        OnFelicaScan = onFelicaScan;
        OnMifareScan = onMifareScan;
        OnInvalidate = onInvalidate;
    }
}
