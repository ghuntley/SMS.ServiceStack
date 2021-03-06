﻿// -----------------------------------------------------------------------
// <copyright file="AppAuthorizations.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Extensions
{
    using System;
    using System.Collections.Generic;

    public class AppAuthorizations
    {
        public AppAuthorizations()
        {
            this.Permissions = new List<string>();
            this.Roles = new List<string>();
            this.IssuedOn = DateTime.UtcNow;
        }

        public DateTime IssuedOn { get; private set; }

        public List<string> Permissions { get; set; }

        public List<string> Roles { get; set; }
    }
}
