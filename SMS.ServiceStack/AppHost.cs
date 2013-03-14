namespace SMS.ServiceStack
{
    using System.Net;
    using System.Reflection;

    using CentralStation.Filter;

    using Funq;

    using SMS.ServiceStack.Authorization;
    using SMS.ServiceStack.Config;
    using SMS.ServiceStack.Log;

    using global::ServiceStack.CacheAccess;
    using global::ServiceStack.CacheAccess.Providers;
    using global::ServiceStack.Configuration;
    using global::ServiceStack.FluentValidation;
    using global::ServiceStack.Logging;
    using global::ServiceStack.Logging.Log4Net;
    using global::ServiceStack.OrmLite;
    using global::ServiceStack.Razor;
    using global::ServiceStack.ServiceInterface;
    using global::ServiceStack.ServiceInterface.Auth;
    using global::ServiceStack.ServiceInterface.Validation;
    using global::ServiceStack.Text;
    using global::ServiceStack.WebHost.Endpoints;

    public abstract class AppHost : AppHostHttpListenerBase
    {
        private readonly IResourceManager appSettings;

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
                new OrmLiteConnectionFactory(connectionString, false, SqlServerDialect.Provider));

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

            this.SetConfig(config);

            this.AddAuthentication(container);
            this.LoadCertificates();
            
            Plugins.Add(new ValidationFeature());

            var cache = new MemoryCacheClient { FlushOnDispose = false };
            container.Register<ICacheClient>(cache);
            this.PreRequestFilters.Add(new TokenFilter(this.appSettings).Filter());
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
                { HtmlRedirect = "/login" });
        }
    }
}
