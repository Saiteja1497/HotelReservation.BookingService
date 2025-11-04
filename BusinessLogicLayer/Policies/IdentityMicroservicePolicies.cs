using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace BusinessLogicLayer.Policies
{
    public class IdentityMicroservicePolicies : IIdentityMicroservicePolicies
    {
        private readonly ILogger<IdentityMicroservicePolicies> _logger;
        private readonly IPollyPolicies _pollyPolicies;
        public IdentityMicroservicePolicies(ILogger<IdentityMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
        {
            _logger = logger;
            _pollyPolicies = pollyPolicies;
        }
        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
            var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
            var timeOutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(3));
            AsyncPolicyWrap<HttpResponseMessage> policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeOutPolicy);
            return policy;
        }
    }
}
