using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Icannsee.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Icannsee.Web.Controllers
{
    [ApiController, Route("lookup")]
    [Produces("application/json")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;
        private readonly ILogger<LookupController> _logger;

        public LookupController(ILookupService service, ILogger<LookupController> logger)
        {
            _lookupService = service;
            _logger = logger;
        }

        [HttpGet("{term}")]
        [SwaggerOperation("Executes a lookup against the specified providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The operation has completed.", typeof(LookupAggregateResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The term or provider was invalid.", typeof(string))]
        public async Task<IActionResult> Lookup(
            [SwaggerParameter("The term to search. May be a valid domain name or IP address.")] string term, 
            [FromQuery, SwaggerParameter("The providers to query. Defaults to all applicable providers.")]string[] providers)
        {
            var query = new LookupQuery() { Providers = providers, QueryText = term };
            if (!query.IsValid())
            {
                return BadRequest("Invalid search term.");
            }

            var providerNames = await _lookupService.GetProviderNamesAsync();
            if (providers.Any(q => !providerNames.Contains(q, StringComparer.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid provider selection.");
            }

            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(30));

            var result = await _lookupService.ExecuteAsync(query, source.Token);
            return Ok(result);
        }

        [HttpGet("providers")]
        [SwaggerOperation("Retrieves the list of available lookup providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of available lookup providers.", typeof(string[]))]
        public async Task<IActionResult> GetProviders()
        {
            return Ok(await _lookupService.GetProviderNamesAsync());
        }
    }
}
