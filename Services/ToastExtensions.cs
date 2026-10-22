namespace Mageki;

public static class ToastExtensions
{
    public static Task DisplayToastAsync(this VisualElement? element, string message)
    {
#if ANDROID
        Android.Widget.Toast.MakeText(Android.App.Application.Context, message, Android.Widget.ToastLength.Short)?.Show();
        return Task.CompletedTask;
#else
        return DisplayAlertFallbackAsync(element, message);
#endif
    }

    private static Task DisplayAlertFallbackAsync(VisualElement? element, string message)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = element as Page
                ?? element?.Window?.Page
                ?? Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page is not null)
            {
                await page.DisplayAlertAsync(string.Empty, message, "OK");
            }
        });
    }
}
