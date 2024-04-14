using Mageki.Drawables;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki
{
    public static class StaticIO
    {
        private static IO io;

        public static Status Status => io.Status;
        public static OutputData Data => io.Data;
        public static ButtonColors[] Colors => io.Colors;

        public static event EventHandler<OnStatusChangedEventArgs> OnStatusChanged;
        public static event EventHandler<EventArgs> OnLedChanged;

        static StaticIO()
        {
            Settings.ValueChanged += Settings_ValueChanged;
            InitIO();
        }

        private static void Settings_ValueChanged(string name)
        {
            if (name == nameof(Settings.Protocol) || name == nameof(Settings.Port) || name == nameof(Settings.IP))
            {
                InitIO();
            }
        }
        private static void InitIO()
        {
            bool protocolChanged = io is null ||
                                   io.GetType().ToString().ToLower() != $"{Settings.Protocol}IO".ToLower();
            bool portChanged;
            {
                portChanged = (io is UdpIO udpIO && udpIO.Port != Settings.Port) ||
                (io is TcpIO tcpIO && tcpIO.Port != Settings.Port);
            }

            bool ipChanged;
            {
                ipChanged = io is UdpIO udpIO && udpIO.IP != Settings.IPAddress;
            }
            if (protocolChanged || portChanged)
            {
                if (io != null)
                {
                    io.OnStatusChanged -= RaiseOnStatusChanged;
                    io.OnLedChanged -= RaiseOnLedChanged;
                    io.Dispose();
                }

                try
                {
                    io = Settings.Protocol switch
                    {
                        Protocol.UDP => new UdpIO(),
                        Protocol.TCP => new TcpIO(),
                        _ => throw new NotImplementedException($"Unsupported protocols:{Settings.Protocol}"),
                    };
                    io.OnStatusChanged += RaiseOnStatusChanged;
                    io.OnLedChanged += RaiseOnLedChanged;
                    io.Init();
                    RaiseOnStatusChanged(io, new OnStatusChangedEventArgs(Status.None, Status.Disconnected));
                    RaiseOnLedChanged(io, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }
            else if (ipChanged)
            {
                if (io is UdpIO udpIO)
                {
                    udpIO.IP = Settings.IPAddress;
                }
            }
        }

        private static void RaiseOnStatusChanged(object sender, OnStatusChangedEventArgs e)
        {
            OnStatusChanged?.Invoke(sender, e);
        }

        private static void RaiseOnLedChanged(object sender, EventArgs e)
        {
            OnLedChanged?.Invoke(sender, e);
        }

        public static void SetGameButton(int index, byte value)
        {
            io.SetGameButton(index, value);
        }
        public static void SetLever(short value)
        {
            io.SetLever(value);
        }
        public static void SetOptionButton(OptionButtons button, bool pressed)
        {
            io.SetOptionButton(button, pressed);
        }

        public static void SetAime(byte scanning, byte[] packet)
        {
            io.SetAime(scanning, packet);
        }
    }
}
