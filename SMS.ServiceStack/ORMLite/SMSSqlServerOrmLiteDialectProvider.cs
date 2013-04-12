// -----------------------------------------------------------------------
// <copyright file="SMSSqlServerOrmLiteDialectProvider.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.ORMLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using SMS.ServiceStack.Types;

    using global::ServiceStack.OrmLite;
    using global::ServiceStack.OrmLite.SqlServer;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SMSSqlServerOrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public static new SMSSqlServerOrmLiteDialectProvider Instance = new SMSSqlServerOrmLiteDialectProvider();

        public override object ConvertDbValue(object value, Type type)
        {
            try
            {
                if (value == null || value is DBNull) return null;

                if (type == typeof(DateTimeUTC))
                {
                    return DateTimeUTC.FromUTCTime((DateTime)value);
                }

                return base.ConvertDbValue(value, type);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(DateTimeUTC))
            {
                var dateValue = (DateTimeUTC)value;
                const string iso8601Format = "yyyyMMdd HH:mm:ss.fff";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
            }

            return base.GetQuotedValue(value, fieldType);


        }
    }
}
