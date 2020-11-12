using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Icannsee
{
    /// <summary>
    /// A container for queries supporting the ability to select
    /// providers to execute.
    /// </summary>
    public class LookupQuery
    {
        public string[] Providers { get; set; }
        public string QueryText { get; set; }

        public LookupQuery() { }

        public LookupQuery(string text)
        {
            QueryText = text;
        }

        public bool IsSelectedProvider(string providerName)
        {
            return Providers?.Any() == false || Providers?.Contains(providerName, StringComparer.OrdinalIgnoreCase) == true;
        }
    }

    public class LookupAggregateResult : List<LookupResult>
    {
        // Reserved for potential global properties
    }

    /// <summary>
    /// An individual result from a lookup provider.
    /// Value is open to support a variety of data types.
    /// </summary>
    public class LookupResult
    {
        public string Provider { get; private set; }
        public string ErrorMessage { get; set; }
        public object Value { get; set; }

        public LookupResult(string providerName)
        {
            Provider = providerName?.ToLower();
        }
    }

    /// <summary>
    /// Extensions for validating and determining the type of query.
    /// </summary>
    public static class LookupExtensions
    {
        private const string DomainSegmentPattern = "[a-z0-9]{1}([a-z0-9-]{1,61}[a-z0-9]{1})?";
        private static readonly Regex DomainPattern = new Regex($"^{DomainSegmentPattern}(\\.{DomainSegmentPattern})*$", RegexOptions.Compiled);

        public static bool IsDomainName(this LookupQuery query)
        {
            return DomainPattern.IsMatch(query.QueryText.ToLower());
        }

        public static bool IsIPAddress(this LookupQuery query)
        {
            return IPAddress.TryParse(query.QueryText, out IPAddress _);
        }

        public static bool IsIPv4(this LookupQuery query)
        {
            return IPAddress.TryParse(query.QueryText, out IPAddress address) && address.AddressFamily == AddressFamily.InterNetwork;
        }

        public static bool IsIPv6(this LookupQuery query)
        {
            return IPAddress.TryParse(query.QueryText, out IPAddress address) && address.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public static bool IsValid(this LookupQuery query)
        {
            return IsDomainName(query) || IsIPAddress(query);
        }
    }
}
