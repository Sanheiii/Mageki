﻿
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
        /// 是否正在刷卡 0：不在刷，1：直接设置Aime卡号，2：设置felica卡号
        /// </summary>
        public byte Scanning { get; set; }
        /// <summary>
        /// Scanning==0时：无意义
        /// Scanning==1时：Mifare以BCD编码存储的AimeId，全255指示读取磁盘中Aime.txt
        /// Scanning==2时：Felica的IDm，PMm，SystemCode顺序连接
        /// </summary>
        public byte[] AimePacket { get; set; } = new byte[10];
        public OptionButtons OptButtons { get; set; }
        public byte[] ToByteArray() => GameButtons
            .Concat(BitConverter.GetBytes(Lever))
            .Concat(new byte[] { (byte)OptButtons })
            .Concat(new byte[] { Scanning })
            .Concat(AimePacket)
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
        private Status status;

        public Status Status
        {
            get => status;
            protected set
            {
                var oldValue = status;
                status = value;
                if (oldValue != value)
                    RaiseOnStatusChanged(new OnStatusChangedEventArgs(oldValue, value));
            }
        }

        public event EventHandler<OnStatusChangedEventArgs> OnStatusChanged;
        protected void RaiseOnStatusChanged(OnStatusChangedEventArgs args)
        {
            OnStatusChanged?.Invoke(this, args);
        }
        public event EventHandler<EventArgs> OnLedChanged;
        protected void RaiseOnLedChanged(EventArgs args)
        {
            OnLedChanged?.Invoke(this, args);
        }

        public abstract void Init();
        public abstract void Dispose();
        /// <summary>
        /// 设置按键状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value">触点数量，0为释放状态</param>
        public virtual void SetGameButton(int index, byte value)
        {
            data.GameButtons[index] = value;
        }
        public virtual void SetLever(short value)
        {
            data.Lever = value;
        }
        public virtual void SetAime(byte scanning, byte[] packet)
        {
            data.Scanning = scanning;
            data.AimePacket = packet;
        }
        public virtual void SetOptionButton(OptionButtons button, bool pressed)
        {
            if (pressed)
                data.OptButtons |= button;
            else
                data.OptButtons &= ~button;
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
    enum MessageType : byte
    {
        // 控制器向IO发送的
        ButtonStatus = 1,
        MoveLever = 2,
        Scan = 3,
        Test = 4,
        Service = 5,
        RequestValues = 6,
        // IO向控制器发送的
        SetLed = 20,
        SetLever = 21,
        // 寻找在线设备
        Hello = 255
    }

    public enum Status
    {
        None,
        Disconnected,
        Connected,
        Error
    }

    public class OnStatusChangedEventArgs : EventArgs
    {
        public Status OldStatus { get; protected set; }
        public Status NewStatus { get; protected set; }
        public OnStatusChangedEventArgs(Status oldStatus, Status newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
