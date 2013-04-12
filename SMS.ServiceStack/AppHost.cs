namespace SMS.ServiceStack
{
    using System;
    using System.Net;
    using System.Reflection;

    using CentralStation.Filter;

    using Funq;

    using SMS.ServiceStack.Authorization;
    using SMS.ServiceStack.Config;
    using SMS.ServiceStack.Log;
    using SMS.ServiceStack.ORMLite;
    using SMS.ServiceStack.Types;

    using global::ServiceStack.CacheAccess;
    using global::ServiceStack.CacheAccess.Providers;
    using global::ServiceStack.Configuration;
    using global::ServiceStack.FluentValidation;
    using global::ServiceStack.Logging;
    using global::ServiceStack.Logging.Log4Net;
    using global::ServiceStack.OrmLite;
    using global::ServiceStack.Razor;
    using global::ServiceStack.ServiceHost;
    using global::ServiceStack.ServiceInterface;
    using global::ServiceStack.ServiceInterface.Auth;
    using global::ServiceStack.ServiceInterface.Validation;
    using global::ServiceStack.Text;
    using global::ServiceStack.WebHost.Endpoints;
    using global::ServiceStack.WebHost.Endpoints.Extensions;
    using global::ServiceStack.WebHost.Endpoints.Support;

    public abstract class AppHost : AppHostHttpListenerBase
    {
        public event Action OnStop;

        protected IResourceManager appSettings;

        protected AppHost()
        {
        }
        
        protected AppHost(Settings settings, string applicationId, Assembly servicesAssembly)
            : base(applicationId, servicesAssembly)
        {
            this.appSettings = new AppSettingsBase(settings);
        }

        public override void Configure(Container container)
        {
            var connectionString = this.appSettings.GetString("Database");

            this.Plugins.Add(new RazorFormat());

            JsConfig.DateHandler = JsonDateHandler.ISO8601;

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(connectionString, SMSSqlServerDialect.Provider));

            Log.CustomAdoNetAppender.StaticConnectionString = connectionString;
            LogManager.LogFactory = new Log4NetFactory(true);

            using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
            {
                db.CreateTableIfNotExists<LogEntry>();
            }

            container.Register<IResourceManager>(this.appSettings);

            var config = new EndpointHostConfig
            {
                CustomHttpHandlers = { { HttpStatusCode.NotFound, new RazorHandler("/notfound") }, }
            };
            config.AllowFileExtensions.Add("jpeg");
            config.AllowFileExtensions.Add("pdf");
            config.AllowFileExtensions.Add("woff");
            config.AllowFileExtensions.Add("eot");
            config.AllowFileExtensions.Add("ttf");

            this.SetConfig(config);

            this.AddAuthentication(container);
            this.LoadCertificates();
            
            Plugins.Add(new ValidationFeature());

            container.Register<IResourceManager>(ast => this.appSettings);

            var cache = new MemoryCacheClient { FlushOnDispose = false };
            container.Register<ICacheClient>(cache);
            this.PreRequestFilters.Add(new TokenFilter(this.appSettings).Filter());
        }

        public override void Stop()
        {
            if (OnStop != null)
            {
                OnStop();
            }
            base.Stop();
        }

        protected override void ProcessRequest(HttpListenerContext context)
        {
            if (string.IsNullOrEmpty(context.Request.RawUrl)) return;

            var operationName = context.Request.GetOperationName();

            var httpReq = new SelfHostedHttpListenerRequestWrapper(operationName, context.Request);
            var httpRes = new HttpListenerResponseWrapper(context.Response);
            var handler = ServiceStackHttpHandlerFactory.GetHandler(httpReq);

            var serviceStackHandler = handler as IServiceStackHttpHandler;
            if (serviceStackHandler != null)
            {
                var restHandler = serviceStackHandler as RestHandler;
                if (restHandler != null)
                {
                    httpReq.OperationName = operationName = restHandler.RestPath.RequestType.Name;
                }
                serviceStackHandler.ProcessRequest(httpReq, httpRes, operationName);
                httpRes.Close();
                return;
            }

            throw new NotImplementedException("Cannot execute handler: " + handler + " at PathInfo: " + httpReq.PathInfo);
        }

        protected void LoadCertificates()
        {
            var tokenCertificate = this.appSettings.Get<string>("TokenCertificate", null);
            var tokenPassword = this.appSettings.Get<string>("TokenPassword", null);
            var resourceCertificate = this.appSettings.Get<string>("ResourceCertificate", null);
            var resourcePassword = this.appSettings.Get<string>("ResourcePassword", null);
            
            if (!string.IsNullOrEmpty(tokenCertificate) && !string.IsNullOrEmpty(tokenPassword))
            {
                Resource.Certificates.LoadTokenCertificate(tokenCertificate, tokenPassword);
            }

            if (!string.IsNullOrEmpty(resourceCertificate) && !string.IsNullOrEmpty(resourcePassword))
            {
                Resource.Certificates.LoadResourceCertificate(resourceCertificate, resourcePassword);
            }
        }

        protected void AddAuthentication(Container container)
        {
            container.Register<IUserAuthRepository>(c =>
                new CustomOrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>()));

            var authRepo = (CustomOrmLiteAuthRepository)container.Resolve<IUserAuthRepository>();
            authRepo.CreateMissingTables();

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] {
                    new CentralStationOAuth2Provider(this.appSettings, this.appSettings.GetString("CentralStationUri")), 
                                    })
                { HtmlRedirect = "/auth/centralstation" });
        }

        internal class SelfHostedHttpListenerRequestWrapper : HttpListenerRequestWrapper, IHttpRequest
        {
            public static string HttpForwardedProtoHeaderName = "X-Forwarded-Proto";

            public string ForwaredProto
            {
                get
                {
                    return this.forwardedProto;
                }
            }

            private readonly string forwardedProto;

            public SelfHostedHttpListenerRequestWrapper(string operationName, HttpListenerRequest request)
                : base(operationName, request)
            {
                this.forwardedProto = request.Headers[HttpForwardedProtoHeaderName];
            }

            public new bool IsSecureConnection
            {
                get
                {
                    return base.Request.IsSecureConnection || (this.ForwaredProto != null && "HTTPS".Equals(this.ForwaredProto, StringComparison.OrdinalIgnoreCase));
                }
            }

            public new string AbsoluteUri
            {
                get
                {
                    return (this.IsSecureConnection)
                               ? "https://" + base.Request.UserHostName + base.Request.RawUrl
                               : "http://" + base.Request.UserHostName + base.Request.RawUrl;
                }
            }
        }
    }
}
