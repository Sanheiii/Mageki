using CoreFoundation;

using CoreNFC;

using Foundation;

using Mageki.DependencyServices;
using Mageki.iOS.DependencyServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIKit;

using Xamarin.Forms;

//参考：https://github.com/ats-y/XamarinFelicaSample/blob/master/NfcSamples.iOS/NfcService/NfcService.cs
[assembly: Dependency(typeof(NfcService))]
namespace Mageki.iOS.DependencyServices
{
    public class NfcService : NFCTagReaderSessionDelegate, INfcService
    {
        Action<byte[]> _onScanAction;
        Action _onInvalidate;
        private NFCTagReaderSession _session;
        public bool ReadingAvailable => NFCReaderSession.ReadingAvailable;
        public void StartReadFelicaId(Action<byte[]> onScanAction, Action onInvalidate)
        {
            _onScanAction = onScanAction;
            _onInvalidate = onInvalidate;

            _session = new NFCTagReaderSession(NFCPollingOption.Iso18092, this, null);
            _session.AlertMessage = "扫描aime卡片，或取消以模拟刷卡";
            _session.BeginSession();
        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            App.Logger.Error($"DidInvalidate. error=[{error}]");
            _session.Dispose();
            _session = null;
            _onInvalidate();
        }
        public override void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            if (tags.Length > 0)
            {
                _onScanAction(tags[0].GetNFCFeliCaTag().CurrentIdm.ToArray());
            }
            session.InvalidateSession();
        }
    }
}