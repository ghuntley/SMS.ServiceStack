using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    using SMS.ServiceStack;

    class Program : ProgramBase<TestHost>
    {
        static void Main(string[] args)
        {
            Config.Dictionary["Database"] =
                "Data Source=.\\SQLExpress;Initial Catalog=Host;Integrated Security=SSPI";
            MainBase(args);
        }
    }
}
