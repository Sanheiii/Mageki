using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Mageki.Platforms.Android.Services;
using Mageki.Services;

namespace Mageki;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.SensorLandscape, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly byte[] aimeKey = { 0x57, 0x43, 0x43, 0x46, 0x76, 0x32 };
    private readonly byte[] banaKey = { 0x60, 0x90, 0xD0, 0x06, 0x32, 0xF5 };

    private PendingIntent? pendingIntent;
    private IntentFilter[]? intentFiltersArray;
    private string[][]? techListsArray;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AndroidEnvironment.UnhandledExceptionRaiser += HandledException;

        base.OnCreate(savedInstanceState);

        try
        {
            pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop), PendingIntentFlags.Mutable);
            intentFiltersArray = new[] { new IntentFilter(NfcAdapter.ActionTechDiscovered) };
            techListsArray = new[] { new[] { "android.nfc.tech.NfcF" }, new[] { "android.nfc.tech.MifareClassic" } };
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
        }

        ProcessActionTechDiscoveredIntent(Intent);
        Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
    }

    private static void HandledException(object? sender, RaiseThrowableEventArgs e)
    {
        App.UnhandledException(e.Exception);
    }

    private void ProcessActionTechDiscoveredIntent(Intent? intent)
    {
        if (intent?.Action != NfcAdapter.ActionTechDiscovered)
        {
            return;
        }

        var service = App.Services.GetService<INfcService>() as NfcService;
        if (service is null)
        {
            return;
        }

        var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
        if (tag is null)
        {
            return;
        }

        var techList = tag.GetTechList();
        if (techList.Contains("android.nfc.tech.NfcF"))
        {
            using var nfc = NfcF.Get(tag);
            var idm = tag.GetId();
            var pmm = nfc.GetManufacturer();
            var systemCode = nfc.GetSystemCode();
            service.OnFelicaScan?.Invoke(idm.Reverse().Concat(pmm.Reverse()).Concat(systemCode.Reverse()).ToArray());
        }
        else if (techList.Contains("android.nfc.tech.MifareClassic"))
        {
            using var mifare = MifareClassic.Get(tag);
            try
            {
                mifare.Connect();
                var sectorIndex = mifare.BlockToSector(2);
                if (mifare.AuthenticateSectorWithKeyB(sectorIndex, aimeKey) ||
                    mifare.AuthenticateSectorWithKeyA(sectorIndex, banaKey))
                {
                    var block = mifare.ReadBlock(2);
                    var accessCode = block[6..];
                    service.OnMifareScan?.Invoke(accessCode);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex);
            }
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        ProcessActionTechDiscoveredIntent(intent);
    }

    protected override void OnPause()
    {
        base.OnPause();
        try
        {
            NfcAdapter.GetDefaultAdapter(this)?.DisableForegroundDispatch(this);
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        try
        {
            if (pendingIntent is not null && intentFiltersArray is not null && techListsArray is not null)
            {
                NfcAdapter.GetDefaultAdapter(this)?.EnableForegroundDispatch(this, pendingIntent, intentFiltersArray, techListsArray);
            }
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
        }
    }
}
