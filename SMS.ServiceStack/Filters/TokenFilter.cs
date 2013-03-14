// -----------------------------------------------------------------------
// <copyright file="BearerFilter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace CentralStation.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;

    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth2;

    using ServiceStack.Configuration;
    using ServiceStack.Logging;
    using ServiceStack.ServiceHost;
    using ServiceStack.ServiceInterface;
    using ServiceStack.ServiceInterface.Auth;

    using SMS.ServiceStack.Resource;

    public class TokenFilter
    {
        private IResourceManager appSettings;

        public TokenFilter(IResourceManager appSettings)
        {
            this.appSettings = appSettings;
        }

        public Action<IHttpRequest, IHttpResponse> Filter()
        {
            return
            (req, resp) =>
                {
                    if (Certificates.HasCertificates)
                    {
                        if (req.Headers.AllKeys.Contains("Authorization"))
                        {
                            var analyzer =
                                new StandardAccessTokenAnalyzer(
                                    (RSACryptoServiceProvider)Certificates.TokenCertificate.PublicKey.Key,
                                    (RSACryptoServiceProvider)Certificates.ResourceCertificate.PrivateKey);

                            var resourceServer = new ResourceServer(analyzer);

                            var token =
                                resourceServer.GetAccessToken(
                                    HttpRequestInfo.Create((HttpListenerRequest)req.OriginalRequest), new string[] { });

                            // Need to run SessionFeature filter since its not executed before this attribute			
                            SessionFeature.AddSessionIdToRequestFilter(req, resp, null);

                            // copy items to cookies
                            foreach (var item in req.Items)
                            {
                                req.Cookies.Add(item.Key, new Cookie(item.Key, (string)item.Value));
                            }

                            var sessionId = req.GetSessionId();

                            using (var cache = req.GetCacheClient())
                            {
                                var session = cache.GetSession(sessionId);
                                session.UserName = token.User;
                                session.UserAuthName = token.User;
                                session.IsAuthenticated = true;
                                session.Permissions = token.Scope.ToList();

                                session.ProviderOAuthAccess = new List<IOAuthTokens>
                                    { new OAuthTokens { Provider = "credentials" } };

                                cache.Add(SessionFeature.GetSessionKey(sessionId), session);
                            }

                            ILog log = LogManager.GetLogger(GetType());
                            log.Debug("OAuth authorization set");
                        }
                    }
                };
        }
    }
}
