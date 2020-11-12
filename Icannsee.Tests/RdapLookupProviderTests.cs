using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Icannsee.Services;
using Icannsee.Services.Lookups;
using Xunit;

namespace Icannsee
{
    public class RdapLookupProviderTests
    {
        [Fact]
        public void Supports_domain_name()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var task = new LookupQuery("localhost");
            Assert.True(provider.IsSupported(task));
        }

        [Fact]
        public void Supports_ip_address()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var task = new LookupQuery("127.0.0.1");
            Assert.True(provider.IsSupported(task));
        }

        [Fact]
        public async Task Successful_domain_lookup()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var result = await provider.ExecuteAsync(new LookupQuery("google.com"));
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async Task Successful_ip_lookup()
        {
            var provider = new RdapLookupProvider(new MockHttpClientFactory());
            var result = await provider.ExecuteAsync(new LookupQuery("8.8.8.8"));
            Assert.Null(result.ErrorMessage);
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
