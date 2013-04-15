// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SMSOAuth2AuthProvider.cs" company="Smart Meter Solutions b.v.">
//   [COPYRIGHT_STATEMENT]
// </copyright>
// <summary>
//   Defines the SMSOAuth2AuthProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SMS.ServiceStack.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;

    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth2;

    using global::ServiceStack.Common;
    using global::ServiceStack.Common.Web;
    using global::ServiceStack.Configuration;
    using global::ServiceStack.Logging;
    using global::ServiceStack.ServiceHost;
    using global::ServiceStack.ServiceInterface;
    using global::ServiceStack.ServiceInterface.Auth;
    using global::ServiceStack.Text;
    using global::ServiceStack.WebHost.Endpoints;

    using SMS.ServiceStack.Extensions;

    using HttpHeaders = global::ServiceStack.Common.Web.HttpHeaders;

    public class CentralStationOAuth2Provider : IAuthProvider
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(AuthProvider));

        private readonly List<string> allScopes = new List<string> { "r/admin", "r/user" };

        public List<string> AllScopes
        {
            get
            {
                return this.allScopes.Select(s => this.applicationId + "/" + s).ToList();
            }
        }

        public const string Name = "centralstation";
        public string provider = "centralstation";
        public string Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        private readonly string applicationId;

        public string ApplicationId { get; set; }

        public string ApplicationSecret { get; set; }

        public string AuthRealm { get; set; }

        public string AuthorizeUrl { get; set; }

        public string RequestTokenUrl { get; set; }

        public string CallbackUrl { get; set; }

        public string TokenInfoUrl { get; set; }

        public TimeSpan? SessionExpiry { get; set; }

        private WebServerClient client;

        private readonly AuthorizationServerDescription authServerDescription;

        public CentralStationOAuth2Provider(IResourceManager appSettings, string realm)
        {
            this.applicationId = appSettings.GetString("ApplicationId");
            this.AuthRealm = realm;
            this.ApplicationId = appSettings.GetString("ApplicationId");
            this.ApplicationSecret = appSettings.GetString("ApplicationSecret");

            this.RequestTokenUrl = realm + "oauth/token";
            this.AuthorizeUrl = realm + "oauth/auth";
            this.TokenInfoUrl = realm + "oauth/tokeninfo";

            this.authServerDescription = new AuthorizationServerDescription
            {
                AuthorizationEndpoint = new Uri(this.AuthorizeUrl),
                TokenEndpoint = new Uri(this.RequestTokenUrl),
                ProtocolVersion = ProtocolVersion.V20
            };

            this.client = new WebServerClient(this.authServerDescription, this.ApplicationId) { ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(this.ApplicationSecret) };
            SessionExpiry = TimeSpan.FromDays(7 * 2);
        }

        public object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            var tokens = Init(authService, ref session, request);

            var code = authService.RequestContext.Get<IHttpRequest>().QueryString["code"];
            var isPreAuthCallback = !code.IsNullOrEmpty() || !request.UserName.IsNullOrEmpty();
            if (!isPreAuthCallback)
            {
                var state = new AuthorizationState();
                var uri = this.CallbackUrl;
                uri = RemoveQueryStringFromUri(uri);
                state.Callback = new Uri(uri);

                foreach (var permission in this.AllScopes)
                {
                    state.Scope.Add(permission);
                }

                var tracker = new AntiForgeryAuthorizationTracker(authService);
                this.client.AuthorizationTracker = tracker;
                var outgoing = this.client.PrepareRequestUserAuthorization(state);
                tracker.SetAuthorizationState(outgoing);

                session.ReferrerUrl = authService.RequestContext.Get<IHttpRequest>().QueryString["redirect"];

                authService.SaveSession(session, this.SessionExpiry);

                var response = new HttpResult(HttpStatusCode.Redirect, "Moved temporarily");
                foreach (string key in outgoing.Headers.Keys)
                {
                    response.Headers.Add(key, outgoing.Headers[key]);
                }
                return response;
            }

            try
            {
                var isHtml =
                        authService.RequestContext.Get<IHttpRequest>().ResponseContentType.MatchesContentType(
                        ContentType.Html);
                IAuthorizationState auth;
                if (!request.UserName.IsNullOrEmpty())
                {
                    if (!request.Password.IsNullOrEmpty())
                    {
                        auth = client.ExchangeUserCredentialForToken(request.UserName, request.Password, this.AllScopes);
                    } else
                    {
                        return (isHtml && authService.RequestContext.GetHeader("Referer") != null) 
                            ? authService.Redirect(authService.RequestContext.GetHeader("Referer").AddHashParam("f", "BadUsernamePassword"))
                            : new HttpResult(HttpStatusCode.Unauthorized);
                    }
                }
                else
                {
                    var r = authService.RequestContext.Get<IHttpRequest>();
                    try
                    {
                        auth = client.ProcessUserAuthorization(HttpRequestInfo.Create(r.HttpMethod, new Uri(r.AbsoluteUri), r.FormData, r.Headers));
                    } 
                    catch (ProtocolException e)
                    {
                        Log.Info("Protocol exception during processing of code", e);
                        return isHtml 
                            ? authService.Redirect(session.ReferrerUrl.AddHashParam("f", "UnauthorizedForApplication")) 
                            : new HttpResult(HttpStatusCode.Unauthorized, "UnauthorizedForApplication");
                    }
                }

                if (auth.AccessToken == null && !request.UserName.IsNullOrEmpty())
                {
                    return(isHtml && authService.RequestContext.GetHeader("Referer") != null)
                        ? authService.Redirect(authService.RequestContext.GetHeader("Referer").AddHashParam("f", "BadUsernamePassword"))
                        : new HttpResult(HttpStatusCode.Unauthorized);
                }
                return this.UseAccessTokenForAuthenticate(authService, session, auth, tokens);

            }
            catch (WebException we)
            {
                var statusCode = ((HttpWebResponse)we.Response).StatusCode;
                if (statusCode == HttpStatusCode.BadRequest)
                {
                    return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "AccessTokenFailed"));
                }
            }

            //Shouldn't get here
            return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "Unknown"));
        }

        protected IOAuthTokens Init(IServiceBase authService, ref IAuthSession session, Auth request)
        {
            if (request != null && !LoginMatchesSession(session, request.UserName))
            {
                //authService.RemoveSession();
                //session = authService.GetSession();
            }

            var requestUri = authService.RequestContext.AbsoluteUri;
            if (this.CallbackUrl.IsNullOrEmpty())
                this.CallbackUrl = requestUri;

            if (session.ReferrerUrl.IsNullOrEmpty())
                session.ReferrerUrl = (request != null ? request.Continue : null)
                    ?? authService.RequestContext.GetHeader("Referer");

            if (session.ReferrerUrl.IsNullOrEmpty())
                session.ReferrerUrl = ServiceStackHttpHandlerFactory.GetBaseUrl() ?? requestUri.Substring(0, requestUri.IndexOf("/", "https://".Length + 1, StringComparison.Ordinal));

            var tokens = session.ProviderOAuthAccess.FirstOrDefault(x => x.Provider == Provider);
            if (tokens == null)
                session.ProviderOAuthAccess.Add(tokens = new OAuthTokens { Provider = Provider });

            authService.SaveSession(session);

            return tokens;
        }

        protected static bool LoginMatchesSession(IAuthSession session, string userName)
        {
            if (userName == null) return false;
            var isEmail = userName.Contains("@");
            if (isEmail)
            {
                if (!userName.EqualsIgnoreCase(session.Email))
                    return false;
            }
            else
            {
                if (!userName.EqualsIgnoreCase(session.UserName))
                    return false;
            }
            return true;
        }

        private object UseAccessTokenForAuthenticate(
            IServiceBase authService, IAuthSession session, IAuthorizationState auth, IOAuthTokens tokens)
        {
            var hc = new HttpClient();
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            var tokenResult = hc.GetAsync(this.TokenInfoUrl).Result.Content.ReadAsStringAsync().Result;
            var tokenDictionary = JsonObject.Parse(tokenResult);
            var tokenInfo = JsonSerializer.DeserializeFromString<AccessToken>(tokenResult);

            var tv = new TokenValidator();
            tv.ValidateToken(tokenInfo, expectedAudience: this.applicationId);

            tokens.AccessTokenSecret = auth.AccessToken;
            tokens.RequestTokenSecret = auth.RefreshToken;
            tokens.RefreshTokenExpiry = auth.AccessTokenExpirationUtc;

            this.OnAuthenticated(authService, session, tokens, tokenDictionary);

            // save permissions after on authenticated, because it removes them;
            session.Roles = tokenInfo.Scope.GetRolesFromScope(this.applicationId);
            session.Permissions = tokenInfo.Scope.GetPermissionsFromScope(this.applicationId);
            session.IsAuthenticated = true;
            authService.SaveSession(session, this.SessionExpiry);

            var redirectUrl = session.ReferrerUrl.AddHashParam("s", "1");
            session.ReferrerUrl = null;

            return authService.Redirect(redirectUrl);
        }

        public virtual void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var userSession = session as AuthUserSession;
            if (userSession != null)
            {
                LoadUserAuthInfo(userSession, tokens, authInfo);
            }

            if (tokens != null)
            {
                authInfo.ForEach((x, y) => tokens.Items[x] = y);
            }

            foreach (var oAuthToken in session.ProviderOAuthAccess)
            {
                var authProvider = AuthService.GetAuthProvider(oAuthToken.Provider);
                if (authProvider == null) continue;
                var userAuthProvider = authProvider as OAuthProvider;
                if (userAuthProvider != null)
                {
                    userAuthProvider.LoadUserOAuthProvider(session, oAuthToken);
                }
            }

            var httpRes = authService.RequestContext.Get<IHttpResponse>();
            if (httpRes != null)
            {
                httpRes.Cookies.AddPermanentCookie(HttpHeaders.XUserAuthId, session.UserAuthId);
            }

            authService.SaveSession(session, SessionExpiry);
            session.OnAuthenticated(authService, session, tokens, authInfo);
        }

        protected void LoadUserAuthInfo(AuthUserSession userSession, IOAuthTokens tokens, System.Collections.Generic.Dictionary<string, string> authInfo)
        {
            try
            {
                tokens.UserName = authInfo["User"];
                this.LoadUserOAuthProvider(userSession, tokens);
            }
            catch (Exception ex)
            {
                Log.Error("Could not retrieve user info for '{0}'".Fmt(tokens.DisplayName), ex);
            }
        }

        public void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userSession = authSession as AuthUserSession;
            if (userSession == null) return;
            userSession.UserName = tokens.UserName;
            userSession.UserAuthName = tokens.UserName;
        }

        private static string RemoveQueryStringFromUri(string uri)
        {
            int index = uri.IndexOf('?');
            if (index > -1)
            {
                uri = uri.Substring(0, index);
            }
            return uri;
        }

        public bool IsAuthorized(IAuthSession session, IOAuthTokens tokens, Auth request = null)
        {
            if (request != null)
            {
                if (!LoginMatchesSession(session, request.UserName)) return false;
            }

            return tokens != null && !string.IsNullOrEmpty(tokens.AccessTokenSecret);
        }

        public object Logout(IServiceBase service, Auth request)
        {
            var session = service.GetSession();
            var referrerUrl = (request != null ? request.Continue : null)
                ?? session.ReferrerUrl
                ?? service.RequestContext.GetHeader("Referer")
                ?? this.CallbackUrl;

            session.OnLogout(service);

            service.RemoveSession();

            if (service.RequestContext.ResponseContentType == ContentType.Html && !String.IsNullOrEmpty(referrerUrl))
                return service.Redirect(referrerUrl.AddHashParam("s", "-1"));

            return new AuthResponse();
        }
    }

    ///<summary>To prevent CSRF attacks for OAuth logins.</summary>
    ///<remarks>This class prevents an attacker from initiating an OAuth login, logging in
    ///with his account, then sending the callback URL to a different user.
    ///
    /// We put the AntiForgery system's FormToken in the OAuth state field.</remarks>
    class AntiForgeryAuthorizationTracker : IClientAuthorizationTracker
    {
        private readonly IServiceBase service;

        private const string CookieName = "ss-id";
        public AntiForgeryAuthorizationTracker(IServiceBase service)
        {
            this.service = service;
        }

        public void SetAuthorizationState(OutgoingWebResponse outgoingResponse)
        {
            outgoingResponse.Headers[HttpResponseHeader.Location] += "&state=" + Uri.EscapeDataString(service.GetSessionId());
        }

        public IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState)
        {
            var cookie = (service.RequestContext.Get<IHttpRequest>()).Cookies[CookieName];
            if (cookie != null && cookie.Value == clientState)
            {
                return new AuthorizationState { Callback = callbackUrl };
            }
            return null;
        }
    }

    public class TokenValidator
    {
        public void ValidateToken(AccessToken token, string expectedAudience)
        {
            if (string.IsNullOrEmpty(token.ClientIdentifier) || token.ClientIdentifier != expectedAudience)
            {
                var e = new HttpException("Tokens with unexpected audience. Expected '{0}', got '{1}' ".Fmt(expectedAudience, token.ClientIdentifier));
                throw e;
            }
        }
    }
}
