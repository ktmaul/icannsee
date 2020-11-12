using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Icannsee.Services.Lookups
{
    /// <summary>
    /// Executes an ICMP request against the specified address.
    /// </summary>
    public class PingLookupProvider : ILookupProvider
    {
        public string ProviderName => "Ping";

        public async Task<LookupResult> ExecuteAsync(LookupQuery task)
        {

            var result = new LookupResult(ProviderName);
            try
            {
                var value = string.Empty;
                if (task.IsIPAddress() || task.IsDomainName())
                {
                    value = task.QueryText;
                }
                else
                {
                    throw new InvalidOperationException("Query must be a valid domain or IP address.");
                }

                var sender = new Ping();
                var reply = await sender.SendPingAsync(value, 30);
                if (reply.Status != IPStatus.Success)
                {
                    result.ErrorMessage = $"Request failed for the following reason: {reply.Status}";
                }
                else
                {
                    result.Value = new PingResponse() { Address = reply.Address.ToString(), Bytes = reply.Buffer.Length, Time = $"{reply.RoundtripTime}ms" };
                }
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

    public class PingResponse
    {
        public string Address { get; set; }
        public long Bytes { get; set; }
        public string Time { get; set; }
    }
}
