
using Mageki.Utils;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Mageki
{
    public class UdpIO : IO
    {
        private UdpClient client;
        private Thread pollThread;
        private byte helloRandomValue;
        private Timer heartbeatTimer = new Timer(400) { AutoReset = true };
        private Timer disconnectTimer = new Timer(1500) { AutoReset = false };
        private bool disposedValue;
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        private IPAddress iP;

        public IPEndPoint RemoteEP { get; private set; }
        public IPAddress IP 
        { 
            get => iP; 
            set
            {
                iP = value;
                if(RemoteEP != null) RemoteEP.Address = value;

            }
        }
        public int Port { get; private set; }

        public UdpIO() : this(Settings.IPAddress, Settings.Port)
        {

        }
        public UdpIO(IPAddress ip, int port)
        {
            IP = ip;
            Port = port;
            RemoteEP = new IPEndPoint(IP, Port);
        }
        public override void Init()
        {
            try
            {
                helloRandomValue = (byte)(new Random().Next() % 255);
                client = new UdpClient();
                heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
                heartbeatTimer.Start();
                disconnectTimer.Elapsed += DisconnectTimer_Elapsed;
                pollThread = new Thread(PollThread);
                pollThread.Start();
                Status = Status.Disconnected;
            }
            catch (Exception e)
            {
                App.Logger.Error(e);
                Status = Status.Error;
            }
        }
        public override void SetGameButton(int index, byte value)
        {
            base.SetGameButton(index, value);
            SendMessage(new byte[] { (byte)MessageType.ButtonStatus, (byte)index, value });
        }
        public override void SetLever(short value)
        {
            base.SetLever(value);
            SendMessage(new byte[] { (byte)MessageType.MoveLever }.Concat(BitConverter.GetBytes(value)).ToArray());
        }
        public override void SetAime(byte scanning, byte[] packet)
        {
            base.SetAime(scanning, packet);
            SendMessage(new byte[] { (byte)MessageType.Scan, Convert.ToByte(scanning) }.Concat(Data.AimePacket).ToArray());
        }
        public override void SetOptionButton(OptionButtons button, bool pressed)
        {
            base.SetOptionButton(button, pressed);
            MessageType type = button switch
            {
                OptionButtons.Test => MessageType.Test,
                OptionButtons.Service => MessageType.Service,
                _ => throw new NotImplementedException(),
            };
            SendMessage(new byte[] { (byte)type, Convert.ToByte(pressed) }.ToArray());
        }
        /// <summary>
        /// 用于接收数据并设置LED
        /// </summary>
        private void PollThread()
        {
            while (!disposedValue)
            {
                try
                {
                    byte[] buffer = client.Receive(ref ep);
                    ParseBuffer(buffer);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }
        }
        // 在没有连接的时候请求连接,有连接时发送心跳保存连接
        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                SendMessage(new byte[] { (byte)MessageType.Hello, helloRandomValue });
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private void DisconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoteEP = new IPEndPoint(IP, Port);
            Status = Status.Disconnected;
        }
        private void SendMessage(byte[] data)
        {
            // 没有连接到就不发送数据
            if (Status != Status.Connected && data[0] != (byte)MessageType.Hello)
            {
                return;
            }
            client.Send(data, data.Length, RemoteEP);
        }


        private void ParseBuffer(byte[] buffer)
        {
            if (disposedValue || (buffer?.Length ?? 0) == 0) return;
            if (buffer[0] == (byte)MessageType.SetLed && buffer.Length == 5)
            {
                uint ledData = BitConverter.ToUInt32(buffer, 1);
                SetLed(ledData);
            }
            else if (buffer[0] == (byte)MessageType.SetLever && buffer.Length == 3)
            {
                Data.Lever = BitConverter.ToInt16(buffer, 1);
            }
            else if (buffer[0] == (byte)MessageType.Hello && buffer.Length == 2 && buffer[1] == helloRandomValue)
            {
                if (Status != Status.Connected)
                {
                    RemoteEP.Address = new IPAddress(ep.Address.GetAddressBytes());
                    RequestValues();
                    Status = Status.Connected;
                }
                disconnectTimer.Stop();
                disconnectTimer.Start();
            }
            //// 用于直接打开测试显示按键
            //Mu3IO._test.UpdateData();
        }
        private void RequestValues()
        {
            SendMessage(new byte[] { (byte)MessageType.RequestValues });
        }
        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (Status == Status.Connected)
                        Status = Status.Disconnected;
                    client.Dispose();
                    disconnectTimer.Dispose();
                    heartbeatTimer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~IO()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public override void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
