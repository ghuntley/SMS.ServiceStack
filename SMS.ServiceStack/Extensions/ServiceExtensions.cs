﻿// -----------------------------------------------------------------------
// <copyright file="JsonServiceClientExtensions.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Extensions
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetOpenAuth.OAuth2;

    using SMS.ServiceStack.Resource;

    using global::ServiceStack.ServiceClient.Web;
    using global::ServiceStack.ServiceInterface;

    using SMS.ServiceStack.Proxies;

    using global::ServiceStack.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ServiceExtensions
    {
        public static TResponse OAuthGet<TResponse>(this IServiceBase service, OAuthProxySettings settings, global::ServiceStack.ServiceHost.IReturn<TResponse> request)
        {
            if (service.GetSession().UserAuthName != null && service.GetSession().ProviderOAuthAccess.Count(p => p.Provider == settings.Provider) > 0)
            {
                var authServerDescription = new AuthorizationServerDescription
                {
                    AuthorizationEndpoint = new Uri(settings.Realm + "oauth/auth"),
                    TokenEndpoint = new Uri(settings.Realm + "oauth/token"),
                    ProtocolVersion = ProtocolVersion.V20
                };

                var webClient = new WebServerClient(authServerDescription, settings.ApplicationId)
                {
                    ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(settings.ApplicationSecret)
                };

                var authState =
                    service.GetSession().ProviderOAuthAccess.Where(p => p.Provider == settings.Provider).Select(
                        p =>
                        new AuthorizationState()
                            {
                                AccessToken = p.AccessTokenSecret,
                                RefreshToken = p.RequestTokenSecret,
                                AccessTokenExpirationUtc = p.RefreshTokenExpiry
                            }).Single();
                var changed = webClient.RefreshAuthorization(authState, new TimeSpan(0, 0, 1, 0));

                if (changed)
                {
                    var session = service.GetSession();
                    var oAuthProvider = session.ProviderOAuthAccess.Single(p => p.Provider == settings.Provider);
                    oAuthProvider.AccessTokenSecret = authState.AccessToken;
                    oAuthProvider.RequestTokenSecret = authState.RefreshToken;
                    oAuthProvider.RefreshTokenExpiry = authState.AccessTokenExpirationUtc;
                    session.Permissions = authState.Scope.ToList();
                    service.SaveSession(session, TimeSpan.FromDays(7 * 2));
                }
            }

            var client = new JsonServiceClient(settings.ServiceBaseUri)
                {
                    LocalHttpWebRequestFilter = (r) =>
                        {
                            if (service.GetSession().UserAuthName != null && service.GetSession().ProviderOAuthAccess.Count(p => p.Provider == settings.Provider) > 0)
                            {
                                var authState =
                                    service.GetSession().ProviderOAuthAccess.Where(p => p.Provider == settings.Provider).Select(
                                        p =>
                                        new AuthorizationState()
                                            {
                                                AccessToken = p.AccessTokenSecret,
                                                RefreshToken = p.RequestTokenSecret,
                                                AccessTokenExpirationUtc = p.RefreshTokenExpiry
                                            }).Single();
                                if (authState != null)
                                {
                                    r.Headers.Add(
                                        "Authorization",
                                        new AuthenticationHeaderValue("Bearer", authState.AccessToken).ToString());
                                }
                            }
                        }
                };
            return client.Get(request);
        }

        public static TResponse AppLinkGet<TResponse>(this OAuthProxySettings settings, global::ServiceStack.ServiceHost.IReturn<TResponse> request, AppAuthorizations authorizations)
        {
            if (Certificates.HasCertificates)
            {
                var rsa = (RSACryptoServiceProvider)Certificates.ResourceCertificate.PublicKey.Key;

                var jsonAuthorizations = JsonSerializer.SerializeToString(authorizations);
                var headerData = rsa.Encrypt(Encoding.ASCII.GetBytes(jsonAuthorizations), false);

                var client = new JsonServiceClient(settings.ServiceBaseUri)
                    {
                        LocalHttpWebRequestFilter =
                            r =>
                            r.Headers.Add(
                                "Authorization",
                                new AuthenticationHeaderValue("AppLink", Convert.ToBase64String(headerData)).ToString())
                    };

                return client.Get(request);
            }
            else
            {
                throw new Exception("No certificates loaded!");
            }
        }
    }
}
