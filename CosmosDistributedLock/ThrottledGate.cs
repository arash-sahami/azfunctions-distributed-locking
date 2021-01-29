using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CosmosDistributedLock
{
    public class ExecutionContext
    {
        public Guid InvocationId { get; set; }
    }
    public abstract class ThrottledGate
    {
        private readonly ILogger logger;

        protected ThrottledGate()
        {
        }

        protected ThrottledGate(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task RunAsync(string id, TimeSpan throttleTime, ExecutionContext executionContext, Func<Task> execution, Func<Task> funcIfThrottled = null)
        {
            var leaseId = executionContext.InvocationId.ToString();
            if (await this.TryAcquireLeaseAsync(id, throttleTime, leaseId))
            {
                try
                {
                    await execution();
                }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to run throttled action");
                    throw;
                }
                finally
                {
                    await this.ReleaseLeaseAsync(id, leaseId);
                }
            }
            else
            {
                if (funcIfThrottled != null)
                    await funcIfThrottled();
            }
        }

        public async Task<T> RunAsync<T>(string id, TimeSpan throttleTime, ExecutionContext executionContext, Func<Task<T>> execution, Func<Task<T>> funcIfThrottled = null)
        {
            var leaseId = executionContext.InvocationId.ToString();
            if (await this.TryAcquireLeaseAsync(id, throttleTime, leaseId))
            {
                try
                {
                    return await execution();
                }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to run throttled action");
                    throw;
                }
                finally
                {
                    await this.ReleaseLeaseAsync(id, leaseId);
                }
            }
            else
            {
                if (funcIfThrottled != null)
                    return await funcIfThrottled();
            }

            return default(T);
        }


        // Cancels an obtained lease
        protected internal abstract Task ReleaseLeaseAsync(string id, string leaseId);

        // Tries to acquire lease for the operation
        protected internal abstract Task<bool> TryAcquireLeaseAsync(string id, TimeSpan throttleTime, string leaseId);
    }
}
