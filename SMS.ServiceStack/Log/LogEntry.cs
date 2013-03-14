// -----------------------------------------------------------------------
// <copyright file="LogEntry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Log
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using global::ServiceStack.DataAnnotations;

    [Alias("Log")]
    public class LogEntry
    {
        public virtual int Id { get; set; }

        public virtual DateTime Date { get; set; }

        [StringLength(255)]
        public virtual string Thread { get; set; }

        [StringLength(50)]
        public virtual string Level { get; set; }

        [StringLength(255)]
        public virtual string Logger { get; set; }

        [StringLength(4000)]
        public virtual string Message { get; set; }

        [StringLength(2000)]
        public virtual string Exception { get; set; }
    }
}
