using Mageki.Drawables;
using Mageki.TouchTracking;

using Plugin.FelicaReader;
using Plugin.FelicaReader.Abstractions;

using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

using Button = Mageki.Drawables.Button;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Slider = Mageki.Drawables.Slider;
using Timer = System.Timers.Timer;

namespace Mageki.IO
{
    public class UdpIO : IO
    {
        UdpClient client;
        byte dkRandomValue;
        Timer heartbeatTimer = new Timer(400) { AutoReset = true };
        Timer disconnectTimer = new Timer(1500) { AutoReset = false };
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, Settings.Port);

        public override bool IsConnected => remoteEP.Address.Address != IPAddress.Broadcast.Address;

        public override void Init()
        {
            dkRandomValue = (byte)(new Random().Next() % 255);
            client = new UdpClient();
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            heartbeatTimer.Start();
            disconnectTimer.Elapsed += DisconnectTimer_Elapsed;
            new Thread(PollThread).Start();
        }

        /// <summary>
        /// 用于接收数据并设置LED
        /// </summary>
        private void PollThread()
        {
            while (true)
            {
                byte[] buffer = client.Receive(ref ep);
                ParseBuffer(buffer);
            }
        } // 在没有连接的时候请求连接,有连接时发送心跳保存连接
        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                SendMessage(new byte[] { (byte)MessageType.DokiDoki, dkRandomValue });
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private void DisconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            remoteEP = new IPEndPoint(IPAddress.Broadcast, Settings.Port);
            logo.Color = SKColors.Gray;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }
        private void SendMessage(byte[] data)
        {
            // 没有连接到就不发送数据
            if (!IsConnected && data[0] != (byte)MessageType.DokiDoki)
            {
                return;
            }
            client.Send(data, data.Length, remoteEP);
        }

        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        private bool requireGenRects = false;

        private void ParseBuffer(byte[] buffer)
        {
            if ((buffer?.Length ?? 0) == 0) return;
            if (buffer[0] == (byte)MessageType.SetLed && buffer.Length == 5)
            {
                uint ledData = BitConverter.ToUInt32(buffer, 1);
                SetLed(ledData);
            }
            else if (buffer[0] == (byte)MessageType.SetLever && buffer.Length == 3)
            {
                short lever = BitConverter.ToInt16(buffer, 1);
                slider.Value = lever;
            }
            else if (buffer[0] == (byte)MessageType.DokiDoki && buffer.Length == 2 && buffer[1] == dkRandomValue)
            {
                if (!IsConnected)
                {
                    remoteEP.Address = new IPAddress(ep.Address.GetAddressBytes());
                    RequestValues();
                    logo.Color = SKColors.Black;
                }
                disconnectTimer.Stop();
                disconnectTimer.Start();
            }
            //// 用于直接打开测试显示按键
            //Mu3IO._test.UpdateData();
        }
        enum MessageType : byte
        {
            // 控制器向IO发送的
            ButtonStatus = 1,
            MoveLever = 2,
            Scan = 3,
            Test = 4,
            RequestValues = 5,
            // IO向控制器发送的
            SetLed = 6,
            SetLever = 7,
            Service = 8,
            // 寻找在线设备
            DokiDoki = 255
        }
    }
}
