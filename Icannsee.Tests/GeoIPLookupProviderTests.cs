using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Icannsee.Services;
using Icannsee.Services.Lookups;
using Xunit;

namespace Icannsee
{
    public class GeoIPLookupProviderTests
    {
        [Fact]
        public void Rejects_host_name()
        {
            var provider = new GeoIPLookupProvider(new MockHttpClientFactory());
            var task = new LookupQuery("localhost");
            Assert.False(provider.IsSupported(task));
        }

        [Fact]
        public void Supports_ip_address()
        {
            var provider = new GeoIPLookupProvider(new MockHttpClientFactory());
            var task = new LookupQuery("127.0.0.1");
            Assert.True(provider.IsSupported(task));
        }

        [Fact]
        public async Task Successful_ipv4_lookup()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var result = await provider.ExecuteAsync(new LookupQuery("8.8.8.8"));
            Assert.Null(result.ErrorMessage);
        }

        /// <summary>
        /// The chosen provider does not support IPv6 queries
        /// </summary>
        [Fact]
        public async Task Fails_ipv6_lookup()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var result = await provider.ExecuteAsync(new LookupQuery("::1"));
            Assert.NotNull(result.ErrorMessage);
        }

        public class MockHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name)
            {
                return new HttpClient();
            }
        }
    }
}
