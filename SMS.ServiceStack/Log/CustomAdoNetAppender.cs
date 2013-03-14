namespace SMS.ServiceStack.Log
{
    using log4net.Appender;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CustomAdoNetAppender : AdoNetAppender
    {
        public static string StaticConnectionString;

        public new string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                base.ConnectionString = StaticConnectionString;
            }
        }
    }
}
