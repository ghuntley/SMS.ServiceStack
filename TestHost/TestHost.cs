// -----------------------------------------------------------------------
// <copyright file="TestHost.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TestHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using SMS.ServiceStack;
    using SMS.ServiceStack.Config;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class TestHost : SMS.ServiceStack.AppHost
    {
        private TestHost()
        {
        }

        public TestHost(Settings settings, string applicationId, Assembly servicesAssembly) : base(settings, applicationId, servicesAssembly)
        {
        }
    }
}
