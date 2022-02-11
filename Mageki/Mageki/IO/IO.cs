
using System;
using System.Linq;

namespace Mageki.IO
{
    public class OutputData
    {
        public byte[] Buttons { get; } = new byte[10];
        public short Lever { get; set; }
        public byte Scanning { get; set; }
        public byte[] AimeId { get; } = new byte[10];
        public OptionButtons OptButtons { get; set; }
        public byte[] ToByteArray() => Buttons
            .Concat(BitConverter.GetBytes(Lever))
            .Concat(BitConverter.GetBytes(Scanning))
            .Concat(AimeId)
            .Concat(BitConverter.GetBytes((byte)OptButtons))
            .ToArray();
    }
    [Flags]
    public enum OptionButtons : byte
    {
        Test = 0b01,
        Service = 0b10
    }

    public abstract class IO : IDisposable
    {
        protected OutputData data = new OutputData();
        public OutputData Data => data;
        public abstract bool IsConnected { get; }

        public event EventHandler<EventArgs> OnConnected;
        public event EventHandler<EventArgs> OnDisconnected;

        public abstract void Init();
        public abstract void Close();
        public abstract void Dispose();
        public void SetGameButton(int index, bool pressed)
        {
            data.Buttons[index] = (byte)(pressed ? 1 : 0);
        }
        public void SetLever(short value)
        {
            data.Lever = value;
        }
        public void SetCardReader(bool scanning, byte[] aimeId)
        {
            data.Scanning = (byte)(scanning ? 1 : 0);
            aimeId.CopyTo(data.AimeId, 0);
        }
        public void SetOptionButton(OptionButtons button,bool pressed)
        {

        }


    }
}
