using System;
using System.Collections.Generic;
using System.Text;
using Icannsee.Services;
using Xunit;

namespace Icannsee
{
    public class LookupQueryTests
    {
        [Fact]
        public void Supports_host_name()
        {
            var task = new LookupQuery("localhost");
            Assert.True(task.IsDomainName());
            Assert.True(task.IsValid());
        }

        [Fact]
        public void Supports_multilevel_name()
        {
            var task = new LookupQuery("gplus.google.com");
            Assert.True(task.IsDomainName());
            Assert.True(task.IsValid());
        }

        [Fact]
        public void Supports_ip_address()
        {
            var task = new LookupQuery("127.0.0.1");
            Assert.True(task.IsIPAddress());
            Assert.True(task.IsValid());
        }

        [Fact]
        public void Supports_ipv4_address()
        {
            var task = new LookupQuery("127.0.0.1");
            Assert.True(task.IsIPv4());
            Assert.True(task.IsValid());
        }

        [Fact]
        public void Supports_ipv6_address()
        {
            var task = new LookupQuery("::1");
            Assert.True(task.IsIPv6());
            Assert.True(task.IsValid());
        }

        [Fact]
        public void Rejects_invalid_domain_name()
        {
            var task = new LookupQuery("-.google.com");
            Assert.False(task.IsDomainName());
            Assert.False(task.IsValid());
        }
    }
}
