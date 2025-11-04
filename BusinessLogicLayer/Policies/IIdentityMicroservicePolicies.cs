using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IIdentityMicroservicePolicies
    {
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
