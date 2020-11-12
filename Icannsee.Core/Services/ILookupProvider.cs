using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Icannsee.Services
{
    /// <summary>
    /// Allows implementation of various lookup services individually
    /// </summary>
    public interface ILookupProvider
    {
        string ProviderName { get; }
        Task<LookupResult> ExecuteAsync(LookupQuery task);
        bool IsSupported(LookupQuery task);
    }
}
