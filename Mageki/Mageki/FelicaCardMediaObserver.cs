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
        private ControllerPanel controllerPanel;

        public FelicaCardMediaObserver(ControllerPanel controllerPanel)
        {
            this.controllerPanel = controllerPanel;
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
                controllerPanel.ScanFelica(byteIdm);
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
