// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace SMS.ServiceStack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Runtime.InteropServices;

    using Mono.Options;

    using SMS.ServiceStack.Config;

    using global::ServiceStack.Configuration;
    using global::ServiceStack.Text;
    using global::ServiceStack.WebHost.Endpoints;

    using log4net;

    public abstract class ProgramBase<T> where T : AppHost
    {
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(T));

        protected static Settings Config = new Settings
            {
                Dictionary =
                    new Dictionary<string, string>()
                        {
                            { "ApplicationId", "Host" },
                            { "ApplicationSecret", "some_password" },
                            { "Database", "Data Source=.\\SQLExpress;Initial Catalog=Host;Integrated Security=SSPI" },
                            { "CentralStationUri", "http://localhost:2337/" },
                            { "ApplicationHostUri", "http://localhost:1337/" }
                        }
            };

        public static event Action OnShutdown;

        protected static int Listen = 1337;

        protected static string Path = "/";

        protected static string WebHostPhysicalPath = "./Web";

        protected static bool ShowHelp = false;

        protected static OptionSet Options = new OptionSet()
                {
                    { "applicationId=", "Application name that can be used to identify the process", a => Config.Dictionary["ApplicationId"] = a },
                    { "applicationSecret=", "Application secret that can be used to identify the process", s => Config.Dictionary["ApplicationSecret"] = s },
                    { "c|contentPath=", "content location (templates, img, ...)", v => WebHostPhysicalPath = v },
                    { "d|database=", "database connectionstring", v => Config.Dictionary["Database"] = Uri.UnescapeDataString(v) },
                    { "l|listen=", "port to listen on", v => Listen = int.Parse(v) },
                    { "p|path=", "path to listen on, omit first slash and add trailing slash", v => Path = v },
                    { "h|help",  "show this message and exit", v => ShowHelp = v != null },
                    { "tokenCertificate=",  "used for access token signing key", v => Config.Dictionary.Add("TokenCertificate", v) },
                    { "tokenPassword=",  "used for access token signing key", v => Config.Dictionary.Add("TokenPassword", v) },
                    { "resourceCertificate=",  "used for resource server encryption key", v => Config.Dictionary.Add("ResourceCertificate", v) },
                    { "resourcePassword=",  "used for resource server encryption key", v => Config.Dictionary.Add("ResourcePassword", v) },
                    { "centralStation=", "CS uri", u => Config.Dictionary["CentralStationUri"] = u },
                    { "applicationHostURI=", "CS uri", u => Config.Dictionary["ApplicationHostUri"] = u }
                };

        protected static void MainBase(string[] args)
        {
            try
            {
                Options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write(".exe: ");
                Console.WriteLine(e.Message);
            }

            if (ShowHelp)
            {
                Console.WriteLine("Options: ");
                Options.WriteOptionDescriptions(Console.Out);
                return;
            }

            EndpointHostConfig.Instance.WebHostUrl = Path;
            EndpointHostConfig.Instance.WebHostPhysicalPath = WebHostPhysicalPath;

            var appHost = (T)Activator.CreateInstance(typeof(T), new object[] { Config, Config.Dictionary["ApplicationId"], typeof(T).Assembly });
            appHost.Init();
            var listenOn = "http://+:{0}{1}".Fmt(Listen, Path);

            //netsh http add urlacl url=http://+:[portnumber]/ user=[username]
            appHost.Start(listenOn);

            OnShutdown += delegate
            {
                Logger.Warn("Stopping application");
                appHost.Stop();
                Environment.Exit(0);
            };

            Kernel32.SetConsoleCtrlHandler(new Kernel32.HandlerRoutine(ConsoleCtrlCheck), true);

            Logger.Warn("AppHost Created at {0}, listening on {1}".Fmt(DateTime.Now, listenOn));
            Logger.Warn("Type Ctrl+C to quit");
            Logger.Warn("-------------------");
            Thread.Sleep(Timeout.Infinite);
        }

        private static bool ConsoleCtrlCheck(Kernel32.CtrlTypes ctrlType)
        {
            OnShutdown();
            return true;
        }
    }

    public static class Kernel32
    {
        #region unmanaged

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
    }
}
