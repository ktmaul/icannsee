using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Icannsee.Services;
using Icannsee.Services.Lookups;
using Xunit;

namespace Icannsee
{
    public class PingLookupProviderTests
    {
        [Fact]
        public void Supports_domain_name()
        {
            var provider = new PingLookupProvider();
            var task = new LookupQuery("localhost");
            Assert.True(provider.IsSupported(task));
        }

        [Fact]
        public void Supports_ip_address()
        {
            var provider = new PingLookupProvider();
            var task = new LookupQuery("127.0.0.1");
            Assert.True(provider.IsSupported(task));
        }

        [Fact]
        public async Task Successful_domain_ping()
        {
            var provider = new PingLookupProvider();
            var result = await provider.ExecuteAsync(new LookupQuery("localhost"));
            var value = Assert.IsType<PingResponse>(result.Value);
            Assert.Equal(32, value.Bytes);
        }

        [Fact]
        public async Task Successful_ip_ping()
        {
            var provider = new PingLookupProvider();
            var result = await provider.ExecuteAsync(new LookupQuery("127.0.0.1"));
            var value = Assert.IsType<PingResponse>(result.Value);
            Assert.Equal(32, value.Bytes);
        }
    }
}
