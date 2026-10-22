using Mopups.Services;

namespace Mageki.Services;

public sealed class SettingsPopupService(SettingsPopup popup) : ISettingsPopupService
{
    private readonly SemaphoreSlim semaphore = new(1, 1);
    public event Action? Showing;

    public async Task ShowAsync()
    {
        await semaphore.WaitAsync();
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (MopupService.Instance.PopupStack.Contains(popup))
                {
                    return;
                }

                Showing?.Invoke();
                await MopupService.Instance.PushAsync(popup);
            });
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
