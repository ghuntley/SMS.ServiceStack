// -----------------------------------------------------------------------
// <copyright file="CustomOrmLiteAuthRepository.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Authorization
{
    using System.Text.RegularExpressions;

    using global::ServiceStack.OrmLite;
    using global::ServiceStack.ServiceInterface.Auth;

    public class CustomOrmLiteAuthRepository : OrmLiteAuthRepository
    {
        public CustomOrmLiteAuthRepository(IDbConnectionFactory factory)
            : base(factory)
        {
            // we don't care, but emty is never ok
            base.ValidUserNameRegEx = new Regex(@"^.+$", RegexOptions.Compiled);
        }
    }
}
