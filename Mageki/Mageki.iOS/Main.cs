﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace Mageki.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            try
            {
                App.InitNLog();
                UIApplication.Main(args, null, typeof(AppDelegate));
            }
            catch (Exception e)
            {
                App.UnhandledException(e);
            }
        }

    }
}
