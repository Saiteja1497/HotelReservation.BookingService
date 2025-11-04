using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IHotelMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
        public IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy();

    }
}
