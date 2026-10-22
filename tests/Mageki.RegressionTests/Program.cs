using System.Net;
using System.Net.Sockets;
using Mageki;
using Mageki.Drawables;
using Mageki.Services;
using SkiaSharp;

if (args is ["--native", var directory])
{
    await NativeIntegration.Run(directory);
    return;
}
if (args is ["--native-tcp", var tcpDirectory])
{
    await NativeIntegration.Run(tcpDirectory, tcp: true);
    return;
}

await TcpRegression();
await UdpRegression();
await StaticIoRegression();
InputRegression();
await IosGestureRegression();
StaticIO.Dispose();
Console.WriteLine("PASS: TCP fragmentation/reconnect/timeout, UDP handshake/LED/input, settings transport reuse, touch/lever/card regressions.");

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
}

static async Task WaitFor(Func<bool> condition, string name)
{
    using var timeout = new CancellationTokenSource(5000);
    while (!condition()) await Task.Delay(10, timeout.Token);
    Console.WriteLine($"PASS: {name}");
}

static int FreePort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static async Task<byte[]> ReadMessage(NetworkStream stream)
{
    using var timeout = new CancellationTokenSource(3000);
    byte[] header = new byte[1];
    await stream.ReadExactlyAsync(header, timeout.Token);
    int length = header[0] switch { 1 or 2 => 2, 3 => 1, 4 or 5 or 255 => 1, 6 => 0, _ => throw new Exception("Unknown message") };
    byte[] payload = new byte[length];
    await stream.ReadExactlyAsync(payload, timeout.Token);
    if (header[0] == 3 && payload[0] != 0)
    {
        byte[] card = new byte[payload[0] == 1 ? 10 : 18];
        await stream.ReadExactlyAsync(card, timeout.Token);
        return [.. header, .. payload, .. card];
    }
    return [.. header, .. payload];
}

static async Task Handshake(NetworkStream stream)
{
    var hello = await ReadMessage(stream);
    Check(hello[0] == 255, "TCP initial heartbeat");
    await stream.WriteAsync(hello);
    bool requested = false;
    for (int i = 0; i < 14; i++)
    {
        var message = await ReadMessage(stream);
        if (message[0] == 255)
        {
            await stream.WriteAsync(message);
            i--;
            continue;
        }
        if (message[0] == 6) requested = true;
    }
    Check(requested, "TCP must request LED/lever after becoming connected");
}

static async Task<byte[]> ReadInput(NetworkStream stream)
{
    while (true)
    {
        var message = await ReadMessage(stream);
        if (message[0] != 255) return message;
        await stream.WriteAsync(message);
    }
}

static async Task TcpRegression()
{
    int port = FreePort();
    using var io = new TcpIO(port);
    io.Init();
    using (var peer = new TcpClient())
    {
        await peer.ConnectAsync(IPAddress.Loopback, port);
        var stream = peer.GetStream();
        await Handshake(stream);
        byte[] led = [20, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0];
        foreach (byte b in led)
        {
            await stream.WriteAsync(new byte[] { b });
            await Task.Delay(3);
        }
        await WaitFor(() => io.Colors[0] == ButtonColors.Red && io.Colors[2] == ButtonColors.Blue, "fragmented TCP LED");
        await stream.WriteAsync(new byte[] { 21, 0x34 });
        await Task.Delay(20);
        await stream.WriteAsync(new byte[] { 0x12 });
        await WaitFor(() => io.Data.Lever == 0x1234, "fragmented TCP lever");
        io.SetGameButton(5, 2);
        var message = await ReadInput(stream);
        Check(message.SequenceEqual(new byte[] { 1, 5, 2 }), "game button bytes");
        io.SetAime(0, []);
        message = await ReadInput(stream);
        Check(message.SequenceEqual(new byte[] { 3, 0 }), "scan release must have no trailing payload");
    }
    await WaitFor(() => io.Status == Status.Disconnected, "TCP remote close");
    await WaitFor(() => io.Colors.All(color => color == ButtonColors.Blank), "disconnect clears gameplay LED state");
    using var replacement = new TcpClient();
    await replacement.ConnectAsync(IPAddress.Loopback, port);
    await Handshake(replacement.GetStream());
    await WaitFor(() => io.Status == Status.Connected, "TCP reconnect after remote close");
    await WaitFor(() => io.Status == Status.Disconnected, "TCP heartbeat timeout");
    io.Dispose();
    using var rebound = new TcpIO(port);
    rebound.Init();
    Check(rebound.Status == Status.Disconnected, "disposed TCP releases listener");
}

static async Task UdpRegression()
{
    using var peer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
    using var io = new UdpIO(IPAddress.Loopback, ((IPEndPoint)peer.Client.LocalEndPoint!).Port);
    io.Init();
    using var timeout = new CancellationTokenSource(5000);
    var hello = await peer.ReceiveAsync(timeout.Token);
    Check(hello.Buffer[0] == 255, "UDP heartbeat");
    await peer.SendAsync(hello.Buffer, hello.RemoteEndPoint, timeout.Token);
    bool requested = false;
    for (int i = 0; i < 14; i++)
    {
        var message = await peer.ReceiveAsync(timeout.Token);
        if (message.Buffer[0] == 255)
        {
            await peer.SendAsync(message.Buffer, message.RemoteEndPoint, timeout.Token);
            i--;
            continue;
        }
        if (message.Buffer[0] == 6) requested = true;
    }
    Check(requested, "UDP must request LED/lever");
    await peer.SendAsync(new byte[] { 20, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0 }, hello.RemoteEndPoint, timeout.Token);
    await WaitFor(() => io.Colors[0] == ButtonColors.Red, "UDP LED reply");
    io.SetOptionButton(OptionButtons.Service, true);
    var pressed = await ReadUdpInput(peer, timeout.Token);
    Check(pressed.Buffer.SequenceEqual(new byte[] { 5, 1 }), "UDP service press");
    io.SetOptionButton(OptionButtons.Service, false);
    var released = await ReadUdpInput(peer, timeout.Token);
    Check(released.Buffer.SequenceEqual(new byte[] { 5, 0 }), "UDP service release");
    await WaitFor(() => io.Status == Status.Disconnected, "UDP timeout");
    await WaitFor(() => io.Colors.All(color => color == ButtonColors.Blank), "UDP disconnect clears gameplay LED state");
}

static async Task<UdpReceiveResult> ReadUdpInput(UdpClient peer, CancellationToken token)
{
    while (true)
    {
        var message = await peer.ReceiveAsync(token);
        if (message.Buffer[0] != 255) return message;
        await peer.SendAsync(message.Buffer, message.RemoteEndPoint, token);
    }
}

static async Task StaticIoRegression()
{
    Settings.Port = (ushort)FreePort();
    Settings.Protocol = Protocol.TCP;
    _ = StaticIO.Status;
    using var peer = new TcpClient();
    await peer.ConnectAsync(IPAddress.Loopback, Settings.Port);
    await Handshake(peer.GetStream());
    var original = StaticIO.Data;
    Settings.IP = "127.0.0.1";
    Settings.Notify(nameof(Settings.IP));
    Check(ReferenceEquals(original, StaticIO.Data), "editing UDP IP must not rebuild TCP");
    Settings.Protocol = Protocol.UDP;
    Settings.Notify(nameof(Settings.Protocol));
    var udp = StaticIO.Data;
    Settings.IP = "127.0.0.2";
    Settings.Notify(nameof(Settings.IP));
    Check(ReferenceEquals(udp, StaticIO.Data), "editing UDP IP must reuse UDP instance");
    Settings.Protocol = Protocol.TCP;
    Settings.Notify(nameof(Settings.Protocol));
    await WaitFor(() => StaticIO.Status == Status.Disconnected, "transport switch releases old sockets");
}

static void InputRegression()
{
    var keyboard = new Keyboard { Size = new SKSize(600, 100), Spacing = 0 };
    keyboard.Update();
    keyboard.HandleTouchPressed(1, new SKPoint(50, 50));
    keyboard.HandleTouchMoved(1, new SKPoint(250, 50));
    Check(keyboard[0].TouchCount == 0 && keyboard[2].TouchCount == 1, "keyboard slide");
    keyboard.HandleTouchCancelled(1);
    Check(Enumerable.Range(0, 6).All(i => keyboard[i].TouchCount == 0), "keyboard cancel");
    keyboard.AntiMisTouch = true;
    keyboard.HandleTouchPressed(2, new SKPoint(50, 50));
    keyboard.HandleTouchMoved(2, new SKPoint(450, 50));
    Check(keyboard[0].TouchCount == 0 && keyboard[4].TouchCount == 1, "anti-mistouch uses current position");
    keyboard.HandleTouchReleased(2);
    var lever = new Lever { Size = new SKSize(400, 200) };
    lever.HandleTouchPressed(3, new SKPoint(100, 100));
    lever.HandleTouchMoved(3, new SKPoint(120, 100));
    lever.HandleTouchReleased(3);
    Check(Math.Abs(lever.Value - 0.1f) < 0.001f, "lever flushes final movement");
    lever.HandleTouchPressed(4, new SKPoint(100, 100));
    lever.HandleTouchMoved(4, new SKPoint(110, 100));
    lever.HandleTouchReleased(4);
    Check(Math.Abs(lever.Value - 0.15f) < 0.001f, "lever next gesture has no stale movement");
    Settings.LeverMoveMode = LeverMoveMode.Absolute;
    lever.HandleTouchPressed(8, new SKPoint(300, 100));
    Check(Math.Abs(lever.Value - 0.5f) < 0.001f, "absolute lever reacts on press");
    lever.HandleTouchReleased(8);
    Settings.LeverMoveMode = LeverMoveMode.Relative;
    var popup = new PopupStub();
    var settings = new SettingButton(new NfcStub(), popup) { Size = new SKSize(40, 40) };
    Settings.AimeId = "01234567890123456789";
    settings.HandleTouchPressed(5, new SKPoint(10, 10));
    Check(StaticIO.Data.AimePacket.SequenceEqual(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45, 0x67, 0x89 }), "Aime BCD with leading zero");
    settings.HandleTouchMoved(5, new SKPoint(80, 80));
    settings.HandleTouchReleased(5);
    Check(StaticIO.Data.Scanning == 0 && popup.Count == 0, "release outside cancels scan/menu");
    settings.HandleTouchPressed(6, new SKPoint(10, 10));
    settings.HandleTouchCancelled(6);
    Check(StaticIO.Data.Scanning == 0 && popup.Count == 0, "cancel must not open settings");
    Settings.AimeId = "-1";
    settings.HandleTouchPressed(7, new SKPoint(10, 10));
    Check(StaticIO.Data.AimePacket.All(b => b == 255), "invalid card uses configured disk card");
    settings.HandleTouchCancelled(7);
    Console.WriteLine("PASS: drawable gestures, final lever movement, Aime BCD and cancellation");
}

static async Task IosGestureRegression()
{
    DeviceInfo.Platform = DevicePlatform.iOS;
    Settings.AimeId = "01234567890123456789";
    var nfc = new NfcSessionStub();
    var popup = new PopupStub();
    var button = new SettingButton(nfc, popup) { Size = new SKSize(40, 40) };
    try
    {
        button.HandleTouchPressed(100, new SKPoint(10, 10));
        button.HandleTouchReleased(100);
        Check(popup.Count == 1 && nfc.Count == 0 && StaticIO.Data.Scanning == 0, "iOS quick tap opens settings without NFC");

        button.HandleTouchPressed(101, new SKPoint(10, 10));
        button.HandleTouchCancelled(101);
        await Task.Delay(1100);
        Check(nfc.Count == 0 && popup.Count == 1, "cancelled iOS hold never starts NFC or settings");

        button.HandleTouchPressed(102, new SKPoint(10, 10));
        await Task.Delay(600);
        button.HandleTouchCancelled(102);
        button.HandleTouchPressed(103, new SKPoint(10, 10));
        await Task.Delay(500);
        Check(nfc.Count == 0, "old hold timer cannot start a new NFC gesture early");
        await WaitFor(() => nfc.Count == 1, "iOS long hold starts one NFC session");
        byte[] card = Enumerable.Range(1, 18).Select(i => (byte)i).ToArray();
        nfc.Felica!(card);
        Check(StaticIO.Data.Scanning == 2 && StaticIO.Data.AimePacket.SequenceEqual(card), "iOS FeliCa scan preserves protocol payload");
        button.HandleTouchReleased(103);
        Check(StaticIO.Data.Scanning == 2 && popup.Count == 1, "finger release cannot end an active NFC scan or open settings");
        await WaitFor(() => StaticIO.Data.Scanning == 0, "NFC scan ends after its display interval");

        button.HandleTouchPressed(104, new SKPoint(10, 10));
        await WaitFor(() => nfc.Count == 2, "iOS NFC can start on the next hold");
        nfc.Invalidated!();
        Check(StaticIO.Data.Scanning == 1 && StaticIO.Data.AimePacket.Length == 10, "iOS cancelled NFC falls back to configured Aime ID");
        await WaitFor(() => StaticIO.Data.Scanning == 0, "simulated NFC fallback releases scanning state");
        Console.WriteLine("PASS: iOS settings-button gestures and scan lifecycle (platform service simulated; not a device test).");
    }
    finally
    {
        DeviceInfo.Platform = DevicePlatform.WinUI;
    }
}

sealed class NfcStub : INfcService
{
    public bool ReadingAvailable => false;
    public void StartReadAime(Action<byte[]> felica, Action<byte[]> mifare, Action invalidate) { }
}

sealed class NfcSessionStub : INfcService
{
    public bool ReadingAvailable => true;
    public int Count { get; private set; }
    public Action<byte[]>? Felica { get; private set; }
    public Action? Invalidated { get; private set; }
    public void StartReadAime(Action<byte[]> felica, Action<byte[]> mifare, Action invalidate)
    {
        Count++;
        Felica = felica;
        Invalidated = invalidate;
    }
}

sealed class PopupStub : ISettingsPopupService
{
    public event Action? Showing;
    public int Count { get; private set; }
    public Task ShowAsync() { Count++; Showing?.Invoke(); return Task.CompletedTask; }
}
