// -----------------------------------------------------------------------
// <copyright file="Settings.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Config
{
    using System.Collections.Generic;

    using global::ServiceStack.Configuration;

    public class Settings : ISettings
    {
        public Settings()
        {
            this.Dictionary = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Dictionary { get; set; }

        public string Get(string key)
        {
            string val = null;
            this.Dictionary.TryGetValue(key, out val);
            return val;
        }
    }
}
