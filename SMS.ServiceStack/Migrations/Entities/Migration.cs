// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Migration.cs" company="Smart Meter Solutions b.v.">
//   [COPYRIGHT_STATEMENT]
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SMS.ServiceStack.Migrations.Entities
{
    using System;

    using global::ServiceStack.DataAnnotations;

    [Alias("Migrations")]
    public class Migration
    {
        public Guid Id { get; set; }

        public int Version { get; set; }

        public int Hash { get; set; }

        public DateTime Time { get; set; }
    }
}
