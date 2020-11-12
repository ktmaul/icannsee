using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Icannsee.Services.Lookups
{
    public class RdapLookupProvider : ILookupProvider
    {
        private readonly IHttpClientFactory _factory;

        public string ProviderName => "RDAP";

        public RdapLookupProvider(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<LookupResult> ExecuteAsync(LookupQuery task)
        {
            var result = new LookupResult(ProviderName);
            try
            {
                var client = _factory.CreateClient("rdap");
                client.BaseAddress = new Uri("https://rdap.arin.net/bootstrap/");

                HttpResponseMessage response;
                if (task.IsIPAddress())
                {
                    response = await client.GetAsync($"ip/{task.QueryText}");
                }
                else if (task.IsDomainName())
                {
                    response = await client.GetAsync($"domain/{task.QueryText}");
                }
                else
                {
                    throw new InvalidOperationException("Query must be a valid domain name or IP address");
                }

                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var document = await JsonDocument.ParseAsync(responseStream);
                result.Value = document.RootElement;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.GetBaseException().Message;
            }

            return result;
        }

        public bool IsSupported(LookupQuery task)
        {
            return task.IsIPAddress() || task.IsDomainName();
        }
    }
}
