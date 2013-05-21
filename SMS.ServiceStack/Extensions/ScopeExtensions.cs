namespace SMS.ServiceStack.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ScopeExtensions
    {
        public const string RoleNamespace = "r";

        public const string PermissionNamespace = "p";
        
        public static List<string> GetRolesFromScope(this HashSet<string> scopes, string clientIdentifier)
        {
            var prefix = string.Format("{0}/{1}/", clientIdentifier, RoleNamespace);
            return scopes.Select(s => s.Replace(prefix, string.Empty)).ToList();
        }

        public static List<string> GetPermissionsFromScope(this HashSet<string> scopes, string clientIdentifier)
        {
            var prefix = string.Format("{0}/{1}/", clientIdentifier, PermissionNamespace);
            return scopes.Select(s => s.Replace(prefix, string.Empty)).ToList();
        }

        public static string TryResolveRequestingClient(this IEnumerable<string> scopes)
        {
            return scopes.Select(s => s.Substring(0, s.IndexOf("/"))).Distinct().SingleOrDefault();
        }
    }
}