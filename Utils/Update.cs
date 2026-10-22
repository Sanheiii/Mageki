using Mageki.Resources;

using Newtonsoft.Json.Linq;

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;




using Microsoft.Maui.Controls;

namespace Mageki.Utils
{
    public static class Update
    {
        public static async Task<CheckVersionResult> CheckUpdateAsync(bool force = false, Page? page = null)
        {
            try
            {
                if (!force) await Task.Delay(5000);
                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "mageki");
                using var response = await client.GetAsync("https://api.github.com/repos/sanheiii/mageki/releases/latest");
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseString);
                // Xamarin packages from the original repository are not upgrades for the MAUI app.
                string extension =
#if ANDROID
                    ".apk";
#elif WINDOWS
                    ".zip";
#else
                    ".ipa";
#endif
                bool compatible = data["assets"]?.Any(asset =>
                    asset.Value<string>("name") is { } name &&
                    name.StartsWith("MagekiNeo", StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) == true;
                if (!compatible) return CheckVersionResult.Unavailable;
                Version current = Version.Parse(VersionTracking.CurrentVersion);
                Version latest = Version.Parse(data.Value<string>("tag_name")!.TrimStart('v', 'V'));
                if (current < latest && (force || Settings.IgnoredVersion < latest))
                {
                    string action = await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var target = page ?? Application.Current?.Windows.FirstOrDefault()?.Page;
                        return target is null ? AppResources.Cancel :
                            await target.DisplayActionSheetAsync(AppResources.NewVersionAvailable, AppResources.Cancel, AppResources.DoNotRemindMeAgain, AppResources.GoToReleasePage);
                    });
                    if (action == AppResources.GoToReleasePage)
                    {
                        await Browser.OpenAsync(data["html_url"].Value<string>(), BrowserLaunchMode.SystemPreferred);
                    }
                    else if (action == AppResources.DoNotRemindMeAgain)
                    {
                        Settings.IgnoredVersion = latest;
                    }
                    return CheckVersionResult.CanUpdate;
                }
                else if (current < latest && Settings.IgnoredVersion >= latest)
                {
                    return CheckVersionResult.Ignored;
                }
                else
                {
                    return CheckVersionResult.Latest;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex);
                return CheckVersionResult.Error;
            }
        }
        public enum CheckVersionResult
        {
            CanUpdate,
            Ignored,
            Latest,
            Unavailable,
            Error
        }
    }
}
