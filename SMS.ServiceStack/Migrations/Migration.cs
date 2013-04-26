// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Migration.cs" company="Smart Meter Solutions b.v.">
//   [COPYRIGHT_STATEMENT]
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SMS.ServiceStack.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Funq;

    public class Migration
    {
        public virtual IEnumerable<string> Sqls
        {
            get
            {
                return new List<string>();
            }
        }

        public virtual Action<Container> Migrate
        {
            get
            {
                return container => { return; };
            }
        }

        public int Version
        {
            get
            {
                return int.Parse(this.GetType().Name.Split('_')[0].Substring(1));
            }
        }

        public int Hash
        {
            get
            {
                var combinedSql = string.Join(string.Empty, this.Sqls);
                return combinedSql.GetHashCode();
            }
        }

        public static List<Migration> All
        {
            get
            {
                return typeof(Migration).Assembly.GetTypes().Where(t => t.BaseType == typeof(Migration)).Select(
                    t => (Migration)Activator.CreateInstance(t)).OrderBy(m => m.Version).ToList();
            }
        }
    }
}
