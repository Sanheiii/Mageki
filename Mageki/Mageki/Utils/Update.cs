using Mageki.Resources;

using Newtonsoft.Json.Linq;

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mageki.Utils
{
    public static class Update
    {
        public static async Task CheckUpdateAsync(bool forcce = false)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "mageki");
                using var response = await client.GetAsync("https://api.github.com/repos/sanheiii/mageki/releases/latest");
                string responseString = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseString);
                Version current = Version.Parse(VersionTracking.CurrentVersion);
                Version latest = Version.Parse(data["tag_name"].Value<string>());
                if (current < latest && (forcce || Settings.IgnoredVersion < latest))
                {
                    string action = await Application.Current.MainPage.DisplayActionSheet(AppResources.NewVersionAvailable, AppResources.Cancel, AppResources.DoNotRemindMeAgain, AppResources.GoToReleasePage);
                    if (action == AppResources.GoToReleasePage)
                    {
                        await Browser.OpenAsync(data["html_url"].Value<string>(), BrowserLaunchMode.External);
                    }
                    else if (action == AppResources.DoNotRemindMeAgain)
                    {
                        Settings.IgnoredVersion = latest;
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
