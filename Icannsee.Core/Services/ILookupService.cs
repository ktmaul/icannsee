using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Icannsee.Services
{
    /// <summary>
    /// Responsible for executing a lookup using <see cref="ILookupProvider"/> instances
    /// and aggregating results.
    /// </summary>
    public interface ILookupService
    {
        /// <summary>
        /// Gets the list of providers available to the lookup service.
        /// </summary>
        Task<IEnumerable<string>> GetProviderNamesAsync();

        /// <summary>
        /// Executes the <see cref="LookupQuery"/> and returns the aggregate result.
        /// </summary>
        Task<LookupAggregateResult> ExecuteAsync(LookupQuery task, CancellationToken token = default);
    }

    /// <summary>
    /// Used by worker nodes to execute lookup providers locally.
    /// </summary>
    public class LocalLookupService : ILookupService
    {
        private readonly IEnumerable<ILookupProvider> _providers;

        public LocalLookupService(IEnumerable<ILookupProvider> providers)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            _providers = providers;
        }

        public Task<IEnumerable<string>> GetProviderNamesAsync()
        {
            return Task.FromResult(_providers.Select(q => q.ProviderName));
        }

        public async Task<LookupAggregateResult> ExecuteAsync(LookupQuery task, CancellationToken token = default)
        {
            var result = new LookupAggregateResult();
            foreach (var provider in _providers.Where(p => p.IsSupported(task) && task.IsSelectedProvider(p.ProviderName)))
            {
                if (token.IsCancellationRequested)
                {
                    result.Add(new LookupResult(provider.ProviderName) { ErrorMessage = "A timeout has occurred." });
                }
                else
                {
                    result.Add(await provider.ExecuteAsync(task));
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Used by front-end nodes to distribute requests to worker nodes using the same endpoints.
    /// A load balanced IP or domain is required to spread the load among worker nodes.
    /// </summary>
    public class RemoteLookupService : ILookupService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IEnumerable<ILookupProvider> _providers;

        public RemoteLookupService(IHttpClientFactory factory, IEnumerable<ILookupProvider> providers)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            _factory = factory;
            _providers = providers;
        }

        public Task<IEnumerable<string>> GetProviderNamesAsync()
        {
            return Task.FromResult(_providers.Select(q => q.ProviderName.ToLower()));
        }

        public Task<LookupAggregateResult> ExecuteAsync(LookupQuery task, CancellationToken token = default)
        {
            var tasks = new List<Task>();
            var result = new LookupAggregateResult();
            var client = _factory.CreateClient("remote-lookups");
            foreach (var provider in _providers.Where(p => p.IsSupported(task) && task.IsSelectedProvider(p.ProviderName)))
            {
                if (token.IsCancellationRequested)
                {
                    result.Add(new LookupResult(provider.ProviderName) { ErrorMessage = "A timeout has occurred." });
                }
                else
                {
                    var providerName = provider.ProviderName;
                    var providerTask = client.GetAsync($"lookup/{task.QueryText}?providers={providerName}").ContinueWith(t =>
                    {
                        if (t.IsFaulted || t.Result?.IsSuccessStatusCode == false)
                        {
                            result.Add(new LookupResult(providerName) { ErrorMessage = "Unable to retrieve results." });
                        }
                        else if (t.IsCanceled)
                        {
                            result.Add(new LookupResult(providerName) { ErrorMessage = "A timeout occurred before results could be obtained." });
                        }
                        else
                        {
                            var jsonResponse = JsonDocument.Parse(t.Result.Content.ReadAsStreamAsync().GetAwaiter().GetResult());
                            result.Add(new LookupResult(providerName) { Value = jsonResponse.RootElement });
                        }
                    });

                    tasks.Add(providerTask);
                }
            }

            Task.WaitAll(tasks.ToArray(), token);
            return Task.FromResult(result);
        }
    }
}
