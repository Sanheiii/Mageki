using CoreFoundation;

using CoreNFC;

using Foundation;

using Mageki.DependencyServices;
using Mageki.iOS.DependencyServices;
using Mageki.Resources;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;

using Xamarin.Forms;

//参考：https://github.com/ats-y/XamarinFelicaSample/blob/master/NfcSamples.iOS/NfcService/NfcService.cs
[assembly: Dependency(typeof(NfcService))]
namespace Mageki.iOS.DependencyServices
{
    public class NfcService : NFCTagReaderSessionDelegate, INfcService
    {
        Action<byte[]> _onFelicaScan;
        Action _onInvalidate;
        private NFCTagReaderSession _session;
        public bool ReadingAvailable => NFCReaderSession.ReadingAvailable;
        public void StartReadAime(Action<byte[]> onFelicaScan, Action<byte[]> onMifareScan, Action onInvalidate)
        {
            _onFelicaScan = onFelicaScan;
            _onInvalidate = onInvalidate;

            _session = new NFCTagReaderSession(NFCPollingOption.Iso18092, this, null);
            _session.AlertMessage = AppResources.BeginNfcSessionAlert;
            _session.BeginSession();
        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            App.Logger.Error($"DidInvalidate. error=[{error}]");
            _session.Dispose();
            _session = null;
            _onInvalidate();
        }
        public override async void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            if (tags.Length > 0)
            {
                NSData idm = tags[0].GetNFCFeliCaTag().CurrentIdm;
                NSData systemCode = tags[0].GetNFCFeliCaTag().CurrentSystemCode;

                await session.ConnectToAsync(tags[0]);

                tags[0].GetNFCFeliCaTag().Polling(systemCode, PollingRequestCode.NoRequest, PollingTimeSlot.Max1, (pmm, requestData, error) =>
                {
                    if (error != null)
                    {
                        App.Logger.Error(error.ToString());
                        throw new Exception(error.ToString());
                    }
                    if (idm.Length != 8 || pmm.Length != 8 || systemCode.Length != 2)
                    {
                        App.Logger.Error(error.ToString());
                        throw new Exception();
                    }

                    _onFelicaScan(idm.Concat(pmm).Concat(systemCode).ToArray());
                    session.InvalidateSession();
                });
            }
        }
    }
}