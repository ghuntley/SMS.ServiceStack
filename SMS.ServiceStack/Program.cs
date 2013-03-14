// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Mono.Options;

    using SMS.ServiceStack.Config;

    using global::ServiceStack.Configuration;
    using global::ServiceStack.Text;
    using global::ServiceStack.WebHost.Endpoints;

    using log4net;

    public abstract class ProgramBase<T> where T : AppHost
    {
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(T));

        protected static Settings config = new Settings();

        protected static string applicationId = "Host";

        protected static int listen = 1337;

        protected static string databaseConnectionString = "Data Source=.\\SQLExpress;Initial%20Catalog=Host;Integrated%20Security=SSPI";

        protected static string path = "/";

        protected static string webHostPhysicalPath = "./Web";

        protected static string centralStationUri = "http://localhost:2337/";

        protected static string applicationSecret = "some_password";

        protected static bool showHelp = false;

        protected static OptionSet Options = new OptionSet()
                {
                    { "applicationId=", "Application name that can be used to identify the process", a => applicationId = a },
                    { "applicationSecret=", "Application secret that can be used to identify the process", s => applicationSecret = s },
                    { "c|contentPath=", "content location (templates, img, ...)", v => webHostPhysicalPath = v },
                    { "d|database=", "database connectionstring", v => databaseConnectionString = v },
                    { "l|listen=", "port to listen on", v => listen = int.Parse(v) },
                    { "p|path=", "path to listen on, omit first slash and add trailing slash", v => path = v },
                    { "h|help",  "show this message and exit", v => showHelp = v != null },
                    { "tokenCertificate=",  "used for access token signing key", v => config.Dictionary.Add("TokenCertificate", v) },
                    { "tokenPassword=",  "used for access token signing key", v => config.Dictionary.Add("TokenPassword", v) },
                    { "resourceCertificate=",  "used for resource server encryption key", v => config.Dictionary.Add("ResourceCertificate", v) },
                    { "resourcePassword=",  "used for resource server encryption key", v => config.Dictionary.Add("ResourcePassword", v) },
                    { "centralStation=", "CS uri", u => centralStationUri = u }
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

            if (showHelp)
            {
                Console.WriteLine("Options: ");
                Options.WriteOptionDescriptions(Console.Out);
                return;
            }

            EndpointHostConfig.Instance.WebHostUrl = path;
            EndpointHostConfig.Instance.WebHostPhysicalPath = webHostPhysicalPath;

            databaseConnectionString = Uri.UnescapeDataString(databaseConnectionString);

            config.Dictionary.Add("ApplicationId", applicationId);
            config.Dictionary.Add("ApplicationSecret", applicationSecret);
            config.Dictionary.Add("CentralStationUri", centralStationUri);
            config.Dictionary.Add("Database", databaseConnectionString);

            var appHost = (T)Activator.CreateInstance(typeof(T), new object[] { config, applicationId, typeof(T).Assembly });
            appHost.Init();
            var listenOn = "http://+:{0}{1}".Fmt(listen, path);

            //netsh http add urlacl url=http://+:[portnumber]/ user=[username]
            appHost.Start(listenOn);

            Console.CancelKeyPress += delegate
            {
                Logger.Warn("Stopping application");
                appHost.Stop();
            };

            Logger.Warn("AppHost Created at {0}, listening on {1}".Fmt(DateTime.Now, listenOn));
            Logger.Warn("Type Ctrl+C to quit");
            Logger.Warn("-------------------");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
