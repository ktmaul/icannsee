using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Icannsee.Services.Lookups
{
    /// <summary>
    /// Looks up geolocation data based on IP address.
    /// Only supports IPv4 with the ip-api service.
    /// </summary>
    public class GeoIPLookupProvider : ILookupProvider
    {
        private readonly IHttpClientFactory _factory;

        public string ProviderName => "GeoIP";

        public GeoIPLookupProvider(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<LookupResult> ExecuteAsync(LookupQuery task)
        {
            var result = new LookupResult(ProviderName);
            try
            {
                var client = _factory.CreateClient("rdap");
                client.BaseAddress = new Uri("http://ip-api.com/json/");

                HttpResponseMessage response;
                if (task.IsIPv4())
                {
                    response = await client.GetAsync(task.QueryText);
                }
                else
                {
                    throw new InvalidOperationException("Query must be a valid IPv4 address.");
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
            return task.IsIPv4();
        }
    }
}
