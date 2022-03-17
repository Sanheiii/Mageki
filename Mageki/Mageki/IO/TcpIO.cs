using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Mageki
{
    public class TcpIO : IO
    {
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream networkStream;
        private bool connecting = false;
        private TimeSpan millisecond = TimeSpan.FromMilliseconds(1);
        private Thread writingThread;
        private Thread readingThread;
        private byte[] _inBuffer = new byte[4];
        private bool disposedValue;
        public override bool IsConnected => client?.Connected ?? false;

        public override void Init()
        {
            IPAddress ip = new IPAddress(new byte[] { 0, 0, 0, 0 });
            listener = new TcpListener(ip, Settings.Port);
            listener.Start();

            writingThread = new Thread(ReadingThread);
            writingThread.Start();
            readingThread = new Thread(WritingThread);
            readingThread.Start();

        }
        public async void Reconnect()
        {
            if (connecting) return;
            connecting = true;
            var newClient = await listener.AcceptTcpClientAsync();
            Disconnect();
            networkStream = newClient.GetStream();
            client = newClient;
            RiseOnConnected(EventArgs.Empty);
            connecting = false;
        }
        private void Disconnect()
        {
            if (IsConnected)
            {
                client?.Dispose();
                networkStream?.Dispose();
                client = null;
                networkStream = null;
            }
        }
        private void SendMessage()
        {
            byte[] buffer = Data.ToByteArray();
            if (!disposedValue)
            {
                try
                {
                    networkStream.Write(buffer, 0, buffer.Length);
                }
                catch
                {
                    Disconnect();
                }
            }
        }
        private void WritingThread()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            TimeSpan compensate = TimeSpan.Zero;
            while (!disposedValue)
            {
                if (!IsConnected)
                {
                    Reconnect();
                    continue;
                }
                TimeSpan time = (sw.Elapsed + compensate) - millisecond;
                if (time < TimeSpan.Zero)
                {
                    continue;
                }
                sw.Restart();
                SendMessage();
                compensate=time;
            }
            sw.Stop();
        }
        /// <summary>
        /// 用于接收数据并设置LED
        /// </summary>
        private void ReadingThread()
        {
            while (!disposedValue)
            {
                if (!IsConnected)
                {
                    continue;
                }
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, 4, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    continue;
                }
                uint ledData = BitConverter.ToUInt32(_inBuffer, 0);
                SetLed(ledData);
            }
        }
        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (IsConnected)
                        RaiseOnDisconnected(EventArgs.Empty);
                    networkStream?.Dispose();
                    client?.Dispose();
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