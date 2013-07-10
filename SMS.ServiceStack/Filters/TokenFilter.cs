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
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth2;

    using SMS.ServiceStack.Extensions;

    using ServiceStack.Configuration;
    using ServiceStack.Logging;
    using ServiceStack.ServiceHost;
    using ServiceStack.ServiceInterface;
    using ServiceStack.ServiceInterface.Auth;

    using SMS.ServiceStack.Resource;

    using ServiceStack.Text;

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
                    ILog log = LogManager.GetLogger(GetType());
                    
                    if (Certificates.HasCertificates)
                    {
                        if (req.Headers.AllKeys.Contains("Authorization"))
                        {
                            var authHeader = AuthenticationHeaderValue.Parse(req.Headers["Authorization"]);

                            // oauth link
                            if (authHeader.Scheme == "Bearer")
                            {

                                var analyzer =
                                    new StandardAccessTokenAnalyzer(
                                        (RSACryptoServiceProvider)Certificates.TokenCertificate.PublicKey.Key,
                                        (RSACryptoServiceProvider)Certificates.ResourceCertificate.PrivateKey);

                                var resourceServer = new ResourceServer(analyzer);

                                var token =
                                    resourceServer.GetAccessToken(
                                        HttpRequestInfo.Create((HttpListenerRequest)req.OriginalRequest),
                                        new string[] { });

                                this.SetSessionValues(
                                    req,
                                    resp,
                                    token.User,
                                    token.Scope.GetRolesFromScope(appSettings.GetString("ApplicationId")),
                                    token.Scope.GetPermissionsFromScope(appSettings.GetString("ApplicationId")));

                                log.Debug("OAuth authorization set");
                            }
                                // Application link authentication
                            else if (authHeader.Scheme == "AppLink")
                            {
                                var rsa = (RSACryptoServiceProvider)Certificates.ResourceCertificate.PrivateKey;
                                var headerData = rsa.Decrypt(Convert.FromBase64String(authHeader.Parameter), false);
                                var jsonAuthorizations = Encoding.ASCII.GetString(headerData);

                                var authorizations = JsonSerializer.DeserializeFromString<AppAuthorizations>(jsonAuthorizations);

                                if (authorizations.IssuedOn < DateTime.UtcNow.AddMinutes(-1))
                                {
                                    return;
                                }

                                this.SetSessionValues(
                                    req, resp, "AppLink", authorizations.Roles, authorizations.Permissions);
                            }
                        }
                    }
                };
        }

        private void SetSessionValues(IHttpRequest req, IHttpResponse resp, string userName, List<string> roles, List<string> permissions)
        {
            // Need to run SessionFeature filter since its not executed before this attribute			
            SessionFeature.AddSessionIdToRequestFilter(req, resp, null);

            // copy items to cookies
            foreach (var item in req.Items)
            {
                req.Cookies.Add(item.Key, new Cookie(item.Key, (string)item.Value));
            }


            var session = req.GetSession();
            session.UserName = userName;
            session.UserAuthName = userName;
            session.IsAuthenticated = true;
            session.Roles = roles;
            session.Permissions = permissions;

            session.ProviderOAuthAccess = new List<IOAuthTokens>
                { new OAuthTokens { Provider = "credentials" } };

            req.SaveSession(session);
        }
    }
}
