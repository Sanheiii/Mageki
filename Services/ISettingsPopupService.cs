namespace Mageki.Services;

public interface ISettingsPopupService
{
    event Action? Showing;
    Task ShowAsync();
}
