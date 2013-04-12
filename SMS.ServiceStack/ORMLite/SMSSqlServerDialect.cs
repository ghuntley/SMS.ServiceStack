// -----------------------------------------------------------------------
// <copyright file="SMSSqlServerOrmLiteDialect.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.ORMLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using global::ServiceStack.OrmLite;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SMSSqlServerDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return SMSSqlServerOrmLiteDialectProvider.Instance; } }
    }
}
