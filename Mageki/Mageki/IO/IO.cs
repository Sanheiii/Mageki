
using Mageki.Drawables;
using Mageki.Utils;

using System;
using System.Linq;
using System.Numerics;
namespace Mageki
{
    public class OutputData
    {
        /// <summary>
        /// {L1,L2,L3,LS,LM,R1,R2,R3,RS,RM}
        /// </summary>
        public byte[] GameButtons { get; } = new byte[10];
        public short Lever { get; set; }
        /// <summary>
        /// 是否正在刷卡
        /// </summary>
        public byte Scanning { get; set; }
        /// <summary>
        /// 以BCD编码存储的AimeId，全255指示读取磁盘中Aime.txt
        /// </summary>
        public byte[] AimeId { get; } = new byte[10];
        public OptionButtons OptButtons { get; set; }
        public byte[] ToByteArray() => GameButtons
            .Concat(BitConverter.GetBytes(Lever))
            .Concat(BitConverter.GetBytes(Scanning))
            .Concat(AimeId)
            .Concat(BitConverter.GetBytes((byte)OptButtons))
            .ToArray();
    }
    public enum OptionButtons : byte
    {
        Test = 0b01,
        Service = 0b10
    }

    public abstract class IO : IDisposable
    {
        protected OutputData data = new OutputData();
        public OutputData Data => data;
        protected ButtonColors[] colors = new ButtonColors[6];
        public ButtonColors[] Colors => colors;
        public abstract bool IsConnected { get; }

        public event EventHandler<EventArgs> OnConnected;
        protected void RiseOnConnected(EventArgs args)
        {
            OnConnected?.Invoke(this, args);
        }
        public event EventHandler<EventArgs> OnDisconnected;
        protected void RaiseOnDisconnected(EventArgs args)
        {
            OnDisconnected?.Invoke(this, args);
        }
        public event EventHandler<EventArgs> OnLedChanged;
        protected void RaiseOnLedChanged(EventArgs args)
        {
            OnLedChanged?.Invoke(this, args);
        }

        public abstract void Init();
        public abstract void Dispose();
        public virtual void SetGameButton(int index, bool pressed)
        {
            data.GameButtons[index] = (byte)(pressed ? 1 : 0);
        }
        public virtual void SetLever(short value)
        {
            data.Lever = value;
        }
        public virtual void SetAime(bool scanning, byte[] bcd)
        {
            bcd = new byte[10 - bcd.Length].Concat(bcd).ToArray();
            data.Scanning = (byte)(scanning ? 1 : 0);
            bcd.CopyTo(data.AimeId, 0);
        }
        public virtual void SetOptionButton(OptionButtons button, bool pressed)
        {
            if (pressed) data.OptButtons ^= button;
            else data.OptButtons |= button;
        }
        public void SetLed(uint data)
        {
            Colors[0] = (ButtonColors)((data >> 23 & 1) << 2 | (data >> 19 & 1) << 1 | (data >> 22 & 1) << 0);
            Colors[1] = (ButtonColors)((data >> 20 & 1) << 2 | (data >> 21 & 1) << 1 | (data >> 18 & 1) << 0);
            Colors[2] = (ButtonColors)((data >> 17 & 1) << 2 | (data >> 16 & 1) << 1 | (data >> 15 & 1) << 0);
            Colors[3] = (ButtonColors)((data >> 14 & 1) << 2 | (data >> 13 & 1) << 1 | (data >> 12 & 1) << 0);
            Colors[4] = (ButtonColors)((data >> 11 & 1) << 2 | (data >> 10 & 1) << 1 | (data >> 9 & 1) << 0);
            Colors[5] = (ButtonColors)((data >> 8 & 1) << 2 | (data >> 7 & 1) << 1 | (data >> 6 & 1) << 0);
            RaiseOnLedChanged(EventArgs.Empty);
        }

    }
}
