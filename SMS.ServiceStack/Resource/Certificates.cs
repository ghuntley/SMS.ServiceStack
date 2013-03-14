// -----------------------------------------------------------------------
// <copyright file="Certificates.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Resource
{
    using System.Security.Cryptography.X509Certificates;

    public static class Certificates
    {
        private static X509Certificate2 tokenCertificate = null;

        private static X509Certificate2 resourceCertificate = null;

        private static bool hasCertificates = false;

        public static X509Certificate2 TokenCertificate
        {
            get
            {
                return tokenCertificate;
            }
        }

        public static X509Certificate2 ResourceCertificate
        {
            get
            {
                return resourceCertificate;
            }
        }

        public static bool HasCertificates
        {
            get
            {
                return hasCertificates;
            }
        }


        public static void LoadTokenCertificate(string file, string password)
        {
            lock(tokenCertificate)
            {
                var cert = new X509Certificate2(file, password);
                tokenCertificate = cert;
                
                if (resourceCertificate != null)
                {
                    hasCertificates = true;
                }
            }
        }

        public static void LoadResourceCertificate(string file, string password)
        {
            lock(resourceCertificate)
            {
                var cert = new X509Certificate2(file, password);
                resourceCertificate = cert;

                if (tokenCertificate != null)
                {
                    hasCertificates = true;
                }
            }
        }
    }
}
