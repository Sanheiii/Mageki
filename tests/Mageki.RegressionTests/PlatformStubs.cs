using System.Net;

// Only platform services are replaced; protocol and drawable implementations are linked unchanged.
namespace Microsoft.Maui.Controls { }

namespace Mageki.Resources { }

namespace Mageki
{
    public static class App
    {
        public static NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();
    }

    public enum Protocol { UDP, TCP }
    public enum LeverMoveMode { Relative, Absolute }

    public static class Settings
    {
        public static Protocol Protocol { get; set; } = Protocol.TCP;
        public static ushort Port { get; set; } = 4354;
        public static string IP { get; set; } = "";
        public static IPAddress IPAddress => IPAddress.TryParse(IP, out var ip) ? ip : IPAddress.Broadcast;
        public static bool EnableHapticFeedback { get; set; }
        public static bool EnableCompositeMode { get; set; }
        public static bool EnableLeverOverflowHandling { get; set; }
        public static LeverMoveMode LeverMoveMode { get; set; }
        public static float LeverSensitivity { get; set; } = 1;
        public static string AimeId { get; set; } = "";
        public static event Action<string>? ValueChanged;
        public static void Notify(string name) => ValueChanged?.Invoke(name);
    }

    public enum DevicePlatform { Android, iOS, WinUI }
    public static class DeviceInfo { public static DevicePlatform Platform { get; set; } = DevicePlatform.WinUI; }
    public enum HapticFeedbackType { Click }
    public sealed class HapticFeedback
    {
        public static HapticFeedback Default { get; } = new();
        public bool IsSupported => false;
        public static void Perform(HapticFeedbackType type) { }
    }

    public static class MainThread
    {
        public static void BeginInvokeOnMainThread(Action action) => action();
        public static Task InvokeOnMainThreadAsync(Action action) { action(); return Task.CompletedTask; }
    }
}

namespace Mageki.TouchTracking { }
