using CoreNFC;
using Foundation;
using Mageki.Resources;
using Mageki.Services;

namespace Mageki.Platforms.iOS.Services;

public sealed class NfcService : NFCTagReaderSessionDelegate, INfcService
{
    private Action<byte[]>? onFelicaScan;
    private Action? onInvalidate;
    private NFCTagReaderSession? session;
    private bool scanned;

    public bool ReadingAvailable => NFCTagReaderSession.ReadingAvailable;

    public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate)
    {
        if (session is not null) return;
        this.onFelicaScan = onFelicaScan;
        this.onInvalidate = onInvalidate;
        scanned = false;

        session = new NFCTagReaderSession(NFCPollingOption.Iso18092, this, null)
        {
            AlertMessage = AppResources.BeginNfcSessionAlert
        };
        session.BeginSession();
    }

    public override void DidInvalidate(NFCTagReaderSession session, NSError error)
    {
        App.Logger.Error($"DidInvalidate. error=[{error}]");
        this.session?.Dispose();
        this.session = null;
        if (!scanned) onInvalidate?.Invoke();
    }

    public override async void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
    {
        if (tags.Length != 1)
        {
            session.RestartPolling();
            return;
        }

        INFCFeliCaTag? felicaTag = tags[0].AsNFCFeliCaTag;
        if (felicaTag is null)
        {
            session.RestartPolling();
            return;
        }

        try
        {
            await session.ConnectToAsync(tags[0]);
            NSData idm = felicaTag.CurrentIdm;
            NSData systemCode = felicaTag.CurrentSystemCode;

            felicaTag.Polling(systemCode, NFCFeliCaPollingRequestCode.NoRequest, NFCFeliCaPollingTimeSlot.Max1, (pmm, requestData, error) =>
            {
                if (error is not null || idm.Length != 8 || pmm.Length != 8 || systemCode.Length != 2)
                {
                    App.Logger.Error(error?.ToString() ?? "Unexpected FeliCa payload length.");
                    session.InvalidateSession();
                    return;
                }

                // The IO protocol stores each FeliCa field in little-endian order.
                scanned = true;
                onFelicaScan?.Invoke(idm.Reverse().Concat(pmm.Reverse()).Concat(systemCode.Reverse()).ToArray());
                session.InvalidateSession();
            });
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
            session.InvalidateSession();
        }
    }
}
