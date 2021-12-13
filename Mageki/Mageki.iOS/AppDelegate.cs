using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Foundation;
using UIKit;

namespace Mageki.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            ForcePermissions("192.168.50.104", 4354).Wait();
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }
        public async static Task ForcePermissions(string ps_IPAddress, int pi_Port)
        {
            try
            {

                IPAddress ipAddress = IPAddress.Parse(ps_IPAddress);
                //This is only done to force the local network permissions access in iOS 14. 
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, pi_Port);

                // Create a TCP/IP socket.  
                var client = new Socket(ipAddress.AddressFamily,
                                    SocketType.Stream, ProtocolType.Tcp);

                await client.ConnectAsync(remoteEndPoint).ConfigureAwait(false);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
