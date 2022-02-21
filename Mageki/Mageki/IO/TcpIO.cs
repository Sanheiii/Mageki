using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Mageki
{
    public class TcpIO : IO
    {
        private TcpClient client;
        private NetworkStream networkStream;
        private IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, Settings.Port);
        private TimeSpan millisecond = TimeSpan.FromMilliseconds(1);
        private Thread writingThread;
        private Thread readingThread;
        private byte[] _inBuffer = new byte[4];
        private bool disposedValue;
        public override bool IsConnected => client?.Connected ?? false;

        public override void Init()
        {
            writingThread = new Thread(ReadingThread);
            writingThread.Start();
            readingThread = new Thread(WritingThread);
            readingThread.Start();

        }
        public void Connect()
        {
            client = new TcpClient(remoteEP);
            networkStream = client.GetStream();
        }
        private void Disconnect()
        {
            if (IsConnected)
            {
                networkStream?.Dispose();
                client?.Dispose();
                client = null;
                networkStream = null;
            }
        }
        private void SendMessage()
        {
            byte[] buffer = Data.ToByteArray();
            networkStream.Write(buffer, 0, buffer.Length);
        }
        private void WritingThread()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!disposedValue)
            {
                if (sw.Elapsed < millisecond)
                {
                    Thread.Sleep(millisecond - sw.Elapsed);
                    sw.Restart();
                }
                if (!IsConnected)
                {
                    continue;
                }
                SendMessage();
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
                    Connect();
                    continue;
                }
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, 4, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    Disconnect();
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