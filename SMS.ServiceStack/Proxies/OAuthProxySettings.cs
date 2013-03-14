// -----------------------------------------------------------------------
// <copyright file="OAuthProxySettings.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Proxies
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class OAuthProxySettings
    {
        public string ServiceBaseUri { get; set; }

        public string Realm { get; set; }

        public string ApplicationId { get; set; }

        public string ApplicationSecret { get; set; }

        public string Provider { get; set; }
    }
}
