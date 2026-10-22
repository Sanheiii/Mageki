using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using Mageki;
using Mageki.Drawables;

internal static class NativeIntegration
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Init();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Connected(int index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Buttons(ref byte left, ref byte right);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OpButtons(ref byte value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Lever(ref short value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Led(byte board, IntPtr data);

    public static async Task Run(string directory, bool tcp = false)
    {
        directory = Path.GetFullPath(directory);
        string configPath = Path.Combine(directory, "mu3input_config.json");
        byte[]? original = File.Exists(configPath) ? File.ReadAllBytes(configPath) : null;
        string previousDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = directory;
        using var reservation = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)reservation.Client.LocalEndPoint!).Port;
        reservation.Dispose();
        File.WriteAllText(configPath, JsonSerializer.Serialize(new
        {
            IO = new[] { new { Type = tcp ? "Tcp" : "Udp", Param = port, Scope = "All" } }
        }));
        try
        {
            var library = NativeLibrary.Load(Path.Combine(directory, "MU3Input.dll"));
            T Export<T>(string name) where T : Delegate =>
                Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
            var init = Export<Init>("mu3_io_init");
            var connected = Export<Connected>("mu3_io_is_connected");
            var buttons = Export<Buttons>("mu3_io_get_gamebtns");
            var opButtons = Export<OpButtons>("mu3_io_get_opbtns");
            var lever = Export<Lever>("mu3_io_get_lever");
            var led = Export<Led>("mu3_io_led_set_colors");
            if (init() != 0) throw new Exception("MU3Input initialization failed.");
            using IO io = tcp ? new TcpIO(port) : new UdpIO(IPAddress.Loopback, port);
            io.Init();
            await Wait(() =>
            {
                short value = 0;
                lever(ref value);
                return io.Status == Status.Connected && connected(0);
            }, $"native MU3Input {(tcp ? "TCP" : "UDP")} connection");
            io.SetGameButton(0, 1);
            io.SetGameButton(7, 1);
            await Wait(() => { byte l = 0, r = 0; buttons(ref l, ref r); return (l & 1) != 0 && (r & 4) != 0; }, "native left/right game buttons");
            io.SetGameButton(0, 0);
            io.SetGameButton(7, 0);
            await Wait(() => { byte l = 0, r = 0; buttons(ref l, ref r); return l == 0 && r == 0; }, "native button release");
            io.SetLever(-12345);
            await Wait(() => { short value = 0; lever(ref value); return value == -12345; }, "native lever");
            io.SetOptionButton(OptionButtons.Test, true);
            io.SetOptionButton(OptionButtons.Service, true);
            await Wait(() => { byte value = 0; opButtons(ref value); return (value & 3) == 3; }, "native Test/Service press");
            io.SetOptionButton(OptionButtons.Test, false);
            io.SetOptionButton(OptionButtons.Service, false);
            await Wait(() => { byte value = 0; opButtons(ref value); return (value & 3) == 0; }, "native Test/Service release");
            byte[] colors = [1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1];
            var pinned = GCHandle.Alloc(colors, GCHandleType.Pinned);
            try { led(1, pinned.AddrOfPinnedObject()); }
            finally { pinned.Free(); }
            await Wait(() => io.Colors[0] == ButtonColors.Red && io.Colors[1] == ButtonColors.Green && io.Colors[2] == ButtonColors.Blue, "native LED feedback");
            Console.WriteLine("PASS: real IOConfig MU3Input.dll integration.");
        }
        finally
        {
            if (original is not null) File.WriteAllBytes(configPath, original);
            else File.WriteAllText(configPath, JsonSerializer.Serialize(new
            {
                IO = new[] { new { Type = "Udp", Param = 4354, Scope = "All" } }
            }));
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static async Task Wait(Func<bool> condition, string name)
    {
        using var timeout = new CancellationTokenSource(5000);
        try
        {
            while (!condition()) await Task.Delay(10, timeout.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Native receiver failed: {name}");
        }
        Console.WriteLine($"PASS: {name}");
    }
}
