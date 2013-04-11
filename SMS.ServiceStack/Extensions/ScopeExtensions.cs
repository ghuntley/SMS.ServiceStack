namespace SMS.ServiceStack.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ScopeExtensions
    {
        public const string RoleNamespace = "s";

        public const string PermissionNamespace = "p";
        
        public static List<string> GetRolesFromScope(this HashSet<string> scopes, string clientIdentifier)
        {
            var prefix = string.Format("{0}/{1}/", clientIdentifier, RoleNamespace);
            return scopes.Where(s => s.StartsWith(prefix)).Select(s => s.Substring(prefix.Length)).ToList();
        }

        public static List<string> GetPermissionsFromScope(this HashSet<string> scopes, string clientIdentifier)
        {
            var prefix = string.Format("{0}/{1}/", clientIdentifier, PermissionNamespace);
            return scopes.Where(s => s.StartsWith(prefix)).Select(s => s.Substring(prefix.Length)).ToList();
        }
    }
}