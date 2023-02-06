using Mageki.Resources;

using Newtonsoft.Json.Linq;

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mageki.Utils
{
    public static class Update
    {
        public static async Task<CheckVersionResult> CheckUpdateAsync(bool force = false)
        {
            try
            {
                await Task.Delay(5000);
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "mageki");
                using var response = await client.GetAsync("https://api.github.com/repos/sanheiii/mageki/releases/latest");
                string responseString = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseString);
                Version current = Version.Parse(VersionTracking.CurrentVersion);
                Version latest = Version.Parse(data["tag_name"].Value<string>());
                if (current < latest && (force || Settings.IgnoredVersion < latest))
                {
                    string action = await Application.Current.MainPage.DisplayActionSheet(AppResources.NewVersionAvailable, AppResources.Cancel, AppResources.DoNotRemindMeAgain, AppResources.GoToReleasePage);
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
                else if (Settings.IgnoredVersion >= current)
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
            Error
        }
    }
}
