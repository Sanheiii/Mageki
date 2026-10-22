using Microsoft.UI.Windowing;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
using XamlWindow = Microsoft.UI.Xaml.Window;
using WinRT.Interop;

namespace Mageki;

internal static class PlatformWindowState
{
    private static XamlWindow? window;

    public static void Register(XamlWindow platformWindow)
    {
        window = platformWindow;
        window.Activated += RegisterShortcuts;
        ApplyFullScreen(Settings.EnableFullScreen);
    }

    private static void RegisterShortcuts(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
    {
        if (window?.Content is { } content)
        {
            window.Activated -= RegisterShortcuts;
            var fullScreen = new KeyboardAccelerator { Key = global::Windows.System.VirtualKey.F11 };
            fullScreen.Invoked += (_, args) =>
            {
                Settings.EnableFullScreen = !Settings.EnableFullScreen;
                args.Handled = true;
            };
            content.KeyboardAccelerators.Add(fullScreen);
            var exitFullScreen = new KeyboardAccelerator { Key = global::Windows.System.VirtualKey.Escape };
            exitFullScreen.Invoked += (_, args) =>
            {
                if (!Settings.EnableFullScreen) return;
                Settings.EnableFullScreen = false;
                args.Handled = true;
            };
            content.KeyboardAccelerators.Add(exitFullScreen);
        }
    }

    public static void ApplyFullScreen(bool fullScreen)
    {
        if (window is null)
        {
            return;
        }

        window.ExtendsContentIntoTitleBar = false;

        var hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetPresenter(fullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Overlapped);
        window.ExtendsContentIntoTitleBar = false;
    }
}
