// -----------------------------------------------------------------------
// <copyright file="ScopeExtensionsTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    using SMS.ServiceStack.Extensions;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScopeExtensionsTests
    {
        [Test]
        public void WhenRolesAreProvided_ShouldFilterAllRoles()
        {
            var Roles = new HashSet<String>()
                {
                    "dev-greenmiles/r/admin",
                    "dev-greenmiles/p/admin",
                    "dev-centralstation/r/admin",
                    "dev-centralstation/p/admin"
                };

            var resultingRoles1 = Roles.GetRolesFromScope("dev-greenmiles");
            var resultingRoles2 = Roles.GetRolesFromScope("dev-centralstation");

            var resultingPer1 = Roles.GetPermissionsFromScope("dev-greenmiles");
            var resultingPer2 = Roles.GetPermissionsFromScope("dev-centralstation");
        }
    }
}
