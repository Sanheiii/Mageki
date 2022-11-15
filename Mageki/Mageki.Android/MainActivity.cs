using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;

using Mageki.DependencyServices;

using System;
using System.Linq;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mageki.Droid
{
    [Activity(Label = "Mageki", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.SensorLandscape, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly byte[] AIME_KEY = { 0x57, 0x43, 0x43, 0x46, 0x76, 0x32 };
        private readonly byte[] BANA_KEY = { 0x60, 0x90, 0xD0, 0x06, 0x32, 0xF5 };

        PendingIntent pendingIntent;
        IntentFilter[] intentFiltersArray;
        string[][] techListsArray;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            App.InitNLog();
            AndroidEnvironment.UnhandledExceptionRaiser += HandledException;
            Rg.Plugins.Popup.Popup.Init(this);
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            try
            {
                pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, this.GetType()).AddFlags(ActivityFlags.SingleTop), PendingIntentFlags.Mutable);
                IntentFilter tech = new IntentFilter(NfcAdapter.ActionTechDiscovered);

                intentFiltersArray = new IntentFilter[] { tech };
                techListsArray = new string[][] { new string[] { "android.nfc.tech.NfcF" }, new string[] { "android.nfc.tech.MifareClassic" } };
            }
            catch (Exception ex) { }
            LoadApplication(new App());
            this.ProcessActionTechDiscoveredIntent(this.Intent);
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Immersive | SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        }
        /// <summary>
        /// 捕获全局异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HandledException(object sender, RaiseThrowableEventArgs e)
        {
            App.UnhandledException(e.Exception);
        }
        private void ProcessActionTechDiscoveredIntent(Intent intent)
        {
            string action = intent.Action;
            if (action != NfcAdapter.ActionTechDiscovered) return;

            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (tag == null) return;
            var techList = tag.GetTechList();
            if (techList.Contains("android.nfc.tech.NfcF"))
            {
                NfcF nfc = NfcF.Get(tag);
                var idm = tag.GetId();
                var pmm = nfc.GetManufacturer();
                var systemCode = nfc.GetSystemCode();
                (DependencyService.Get<INfcService>() as DependencyServices.NfcService).OnFelicaScan(idm.Concat(pmm).Concat(systemCode).ToArray());
            }
            else if (techList.Contains("android.nfc.tech.MifareClassic"))
            {
                using var mifare = MifareClassic.Get(tag);
                try
                {
                    mifare.Connect();
                    var sectorIndex = mifare.BlockToSector(2);
                    if (mifare.AuthenticateSectorWithKeyB(sectorIndex, AIME_KEY) ||
                        mifare.AuthenticateSectorWithKeyA(sectorIndex, BANA_KEY))
                    {
                        var block = mifare.ReadBlock(2);
                        var accessCode = block[6..];

                        (DependencyService.Get<INfcService>() as DependencyServices.NfcService).OnMifareScan(accessCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ProcessActionTechDiscoveredIntent(intent);
            return;
        }
        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                NfcAdapter.GetDefaultAdapter(this).DisableForegroundDispatch(this);
            }
            catch (Exception ex) { }
        }

        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                NfcAdapter.GetDefaultAdapter(this).EnableForegroundDispatch(this, pendingIntent, intentFiltersArray, techListsArray);
            }
            catch (Exception ex) { }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }
    }
}