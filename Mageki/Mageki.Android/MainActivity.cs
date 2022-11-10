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
                techListsArray = new string[][] { new string[] { "android.nfc.tech.NfcF" } };
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
            if (action != NfcAdapter.ActionTechDiscovered)
            {
                return;
            }

            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (tag != null)
            {
                NfcF nfc = NfcF.Get(tag);
                var idm = tag.GetId();
                var pmm = nfc.GetManufacturer();
                var systemCode = nfc.GetSystemCode();
                (DependencyService.Get<INfcService>() as DependencyServices.NfcService).OnScanAction(idm.Concat(pmm).Concat(systemCode).ToArray());

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