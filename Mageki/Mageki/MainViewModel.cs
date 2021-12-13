using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mageki
{
    [AddINotifyPropertyChangedInterface]
    internal class MainViewModel
    {
        private short lever = 0;

        public double Lever
        {
            get => lever / (double)short.MaxValue;
            set
            {
                lever = (short)(short.MaxValue * (value-0.5));
                Debug.WriteLine(lever);
            }
        }
        public MainViewModel()
        {

        }
    }
}
