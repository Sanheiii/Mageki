using Plugin.FelicaReader.Abstractions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mageki
{
    class FelicaCardMediaObserver : IObserver<IFelicaCardMedia>
    {
        private Action<byte[]> action;

        public FelicaCardMediaObserver(Action<byte[]> action)
        {
            this.action = action;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            Debug.WriteLine(error.Message);
        }

        public async void OnNext(IFelicaCardMedia falicaCardMedia)
        {
            try
            {
                var byteIdm = await falicaCardMedia.GetIdm();
                action.Invoke(byteIdm);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                falicaCardMedia.Dispose();
            }
        }
    }
}
