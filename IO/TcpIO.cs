using System.Net;
using System.Net.Sockets;
using Timer = System.Timers.Timer;

namespace Mageki;

public class TcpIO : IO
{
    private readonly object connectionLock = new();
    private readonly CancellationTokenSource lifetime = new();
    private readonly Timer heartbeatTimer = new(400) { AutoReset = true };
    private readonly Timer disconnectTimer = new(1500) { AutoReset = false };
    private readonly byte helloRandomValue = (byte)Random.Shared.Next(256);
    private TcpListener? listener;
    private TcpClient? client;
    private NetworkStream? stream;
    private bool disposed;

    public int Port { get; }

    public TcpIO() : this(Settings.Port) { }

    public TcpIO(int port) => Port = port;

    public override void Init()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Status = Status.Disconnected;
            heartbeatTimer.Elapsed += (_, _) => SendMessage([(byte)MessageType.Hello, helloRandomValue]);
            disconnectTimer.Elapsed += (_, _) => Disconnect();
            heartbeatTimer.Start();
            _ = PollAsync();
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
            Status = Status.Error;
        }
    }

    private async Task PollAsync()
    {
        byte[] buffer = new byte[18];
        while (!lifetime.IsCancellationRequested)
        {
            try
            {
                var next = await listener!.AcceptTcpClientAsync(lifetime.Token);
                lock (connectionLock)
                {
                    if (disposed)
                    {
                        next.Dispose();
                        return;
                    }
                    client = next;
                    client.NoDelay = true;
                    stream = next.GetStream();
                    disconnectTimer.Start();
                }
                var input = next.GetStream();
                while (!lifetime.IsCancellationRequested)
                {
                    await input.ReadExactlyAsync(buffer.AsMemory(0, 1), lifetime.Token);
                    var type = (MessageType)buffer[0];
                    int length = type switch
                    {
                        MessageType.SetLed => 18,
                        MessageType.SetLever => 2,
                        MessageType.Hello => 1,
                        _ => throw new InvalidDataException($"Unknown IO message: {type}")
                    };
                    await input.ReadExactlyAsync(buffer.AsMemory(0, length), lifetime.Token);
                    if (type == MessageType.SetLed)
                    {
                        SetLed(buffer);
                    }
                    else if (type == MessageType.SetLever)
                    {
                        ReceiveLever(BitConverter.ToInt16(buffer, 0));
                    }
                    else if (buffer[0] == helloRandomValue)
                    {
                        lock (connectionLock)
                        {
                            if (disposed || client != next) break;
                            disconnectTimer.Stop();
                            disconnectTimer.Start();
                            if (Status != Status.Connected)
                            {
                                Status = Status.Connected;
                                SendMessage([(byte)MessageType.RequestValues]);
                                SendCurrentInputs();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException or ObjectDisposedException)
            {
                // Closing a peer, timing out, and disposal all return to the accept loop.
            }
            finally
            {
                Disconnect();
            }
        }
    }

    public void Reconnect() => Disconnect();

    private void Disconnect()
    {
        lock (connectionLock)
        {
            if (!disposed) disconnectTimer.Stop();
            bool hadClient = client is not null;
            stream = null;
            client?.Dispose();
            client = null;
            if (Status != Status.Error) Status = Status.Disconnected;
            if (hadClient) ClearLed();
        }
    }

    public override void SetGameButton(int index, byte value)
    {
        base.SetGameButton(index, value);
        SendMessage([(byte)MessageType.ButtonStatus, (byte)index, value]);
    }

    public override void SetLever(short value)
    {
        base.SetLever(value);
        SendMessage([(byte)MessageType.MoveLever, .. BitConverter.GetBytes(value)]);
    }

    public override void SetAime(byte scanning, byte[] packet)
    {
        base.SetAime(scanning, packet);
        SendMessage([(byte)MessageType.Scan, scanning, .. packet]);
    }

    public override void SetOptionButton(OptionButtons button, bool pressed)
    {
        base.SetOptionButton(button, pressed);
        SendMessage([(byte)(button == OptionButtons.Test ? MessageType.Test : MessageType.Service), (byte)(pressed ? 1 : 0)]);
    }

    private void SendCurrentInputs()
    {
        for (int i = 0; i < Data.GameButtons.Length; i++)
            SendMessage([(byte)MessageType.ButtonStatus, (byte)i, Data.GameButtons[i]]);
#if !WINDOWS
        SendMessage([(byte)MessageType.Test, (byte)((Data.OptButtons & OptionButtons.Test) != 0 ? 1 : 0)]);
        SendMessage([(byte)MessageType.Service, (byte)((Data.OptButtons & OptionButtons.Service) != 0 ? 1 : 0)]);
#endif
        SendMessage([(byte)MessageType.Scan, Data.Scanning, .. Data.AimePacket]);
    }

    private void SendMessage(byte[] message)
    {
        lock (connectionLock)
        {
            if (disposed || stream is null || (Status != Status.Connected && message[0] != (byte)MessageType.Hello))
                return;
            try
            {
                stream.Write(message);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
            {
                Disconnect();
            }
        }
    }

    public override void Dispose()
    {
        lock (connectionLock)
        {
            if (disposed) return;
            disposed = true;
            lifetime.Cancel();
            heartbeatTimer.Dispose();
            disconnectTimer.Dispose();
            listener?.Stop();
            Disconnect();
        }
    }
}
