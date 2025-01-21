using System;
using System.Data;

namespace H10.Shared
{
    public static class DomainNameHandler
    {
        public static string GetSubDomain(string value) {
            if (String.IsNullOrEmpty(value) == true)
                throw new InvalidConstraintException("Unable to resolve subdomain without host information");

            string subDomainPart;

            if (value.Contains("localhost") == true)
                subDomainPart = "localhost";
            else
            {
                string[] domainParts = value.Split('.');

                if (domainParts.Length == 3)
                    subDomainPart = domainParts[0];
                else throw new InvalidOperationException("Domain name not well formed");
            }

            return subDomainPart;
        }
    }
}
