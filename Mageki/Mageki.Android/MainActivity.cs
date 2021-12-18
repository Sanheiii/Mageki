using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Views;
using Android.Nfc;
using Plugin.FelicaReader.Abstractions;
using Plugin.FelicaReader;
using Android.Content;
using Android.Nfc.Tech;
using System.Reactive.Subjects;

namespace Mageki.Droid
{
    [Activity(Label = "Mageki", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.SensorLandscape, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [IntentFilter(new[] { NfcAdapter.ActionTechDiscovered })]
    [MetaData(NfcAdapter.ActionTechDiscovered, Resource = "@xml/nfc_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IFelicaReader felicaReader;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            try
            {
                CrossFelicaReader.Init(this, GetType());
                this.felicaReader = CrossFelicaReader.Current;
            }
            catch (Exception ex) { }
            LoadApplication(new App());
            this.ProcessActionTechDiscoveredIntent(this.Intent);
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Immersive | SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        }

        private void ProcessActionTechDiscoveredIntent(Intent intent)
        {
            string action = intent.Action;
            if (action != NfcAdapter.ActionTechDiscovered)
            {
                return;
            }

            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
            if (tag != null)
            {
                var subject = this.felicaReader.WhenCardFound() as Subject<IFelicaCardMedia>;
                NfcF nfc = NfcF.Get(tag);
                nfc.Connect();
                subject.OnNext(new FelicaCardMediaImplementation(nfc));
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
                this.felicaReader.DisableForeground();
            }
            catch (Exception ex) { }
        }

        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                this.felicaReader.EnableForeground();
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