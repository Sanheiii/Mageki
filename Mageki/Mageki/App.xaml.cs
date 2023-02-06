using Newtonsoft.Json.Linq;

using NLog;
using NLog.Config;
using NLog.Targets;

using System;
using System.Net.Http;
using System.IO;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Mageki.Resources;
using System.Threading.Tasks;

namespace Mageki
{
    public partial class App : Application
    {
        /// <summary>
        /// 用于记录日志
        /// </summary>
        public static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        public static string LogFileName { get; } = DateTime.Now.ToString("yyyyMMdd");

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        /// <summary>
        /// 载入日志框架
        /// </summary>
        public static void InitNLog()
        {
            bool flag = true;
            var config = new LoggingConfiguration();
            var logFileName = Path.Combine(FileSystem.CacheDirectory, "logs", $"{App.LogFileName}.log");
            if (File.Exists(logFileName)) flag = false;
            var logFile = new FileTarget("logFile") { FileName = logFileName };
            var errorFile = new FileTarget("errorFile") { FileName = Path.Combine(FileSystem.CacheDirectory, "logs", $"errors.log") };
            var logConsole = new ConsoleTarget("logConsole");

            config.AddRule(LogLevel.Debug, LogLevel.Off, logFile);
            config.AddRule(LogLevel.Error, LogLevel.Off, errorFile);
            config.AddRule(LogLevel.Trace, LogLevel.Off, logConsole);
            LogManager.Configuration = config;

            //if (flag) LogDeviceInfo();
        }
        /// <summary>
        /// 记录设备信息
        /// </summary>
        public static void LogDeviceInfo()
        {
            Logger.Info($"DeviceType:{DeviceInfo.DeviceType}");
            Logger.Info($"Idiom:{DeviceInfo.Idiom}");
            Logger.Info($"Manufacturer:{DeviceInfo.Manufacturer}");
            Logger.Info($"Model:{DeviceInfo.Model}");
            Logger.Info($"Name:{DeviceInfo.Name}");
            Logger.Info($"Platform:{DeviceInfo.Platform}");
            Logger.Info($"PlatFormVersion:{DeviceInfo.Version}");
            Logger.Info($"ApplicationVersion:{VersionTracking.CurrentVersion}");
        }
        /// <summary>
        /// 捕获全局异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void UnhandledException(Exception e)
        {
            App.Logger.Fatal(e);
            string logString = e.ToString();
            JObject deviceInfo = new JObject()
            {
                new JProperty("DeviceType", DeviceInfo.DeviceType.ToString()) ,
                new JProperty("Idiom", DeviceInfo.Idiom.ToString()) ,
                new JProperty("Manufacturer",DeviceInfo.Manufacturer.ToString()) ,
                new JProperty("Model", DeviceInfo.Model.ToString()) ,
                new JProperty("Name", DeviceInfo.Name.ToString()) ,
                new JProperty("Platform", DeviceInfo.Platform.ToString()) ,
                new JProperty("PlatFormVersion", DeviceInfo.Version.ToString()) ,
                new JProperty("ApplicationVersion", VersionTracking.CurrentVersion.ToString()) ,
            };
            JObject json = new JObject()
            {
                new JProperty("DeviceInfo", deviceInfo) ,
                new JProperty("Exception", logString) ,
            };
            File.WriteAllText(Path.Combine(FileSystem.CacheDirectory, "crash.json"), json.ToString());
            return;
        }

        public static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            UnhandledException(unobservedTaskExceptionEventArgs.Exception);
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            UnhandledException(unhandledExceptionEventArgs.ExceptionObject as Exception);
        }

        protected override async void OnStart()
        {
            string crashFilePath = Path.Combine(FileSystem.CacheDirectory, "crash.json");
            if (File.Exists(crashFilePath))
            {
                try
                {
                    string text = File.ReadAllText(crashFilePath);
                    string message = JObject.Parse(text)["Exception"].Value<string>();
                    //在这里处理上次造成崩溃的异常
                    bool copy = await MainPage.DisplayAlert(AppResources.ProgramCrashedUnexpectedly, message, AppResources.Copy, AppResources.Cancel);
                    if (copy)
                    {
                        await Clipboard.SetTextAsync(text);
                    }
                    File.Delete(crashFilePath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
