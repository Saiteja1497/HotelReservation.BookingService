using DataAccessLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.Policies
{
    public class HotelMicroservicePolicies : IHotelMicroservicePolicies
    {
        private readonly ILogger<HotelMicroservicePolicies> _logger;

        public HotelMicroservicePolicies(ILogger<HotelMicroservicePolicies> logger)
        {
            _logger = logger;
        }
        public IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy()
        {
            AsyncBulkheadPolicy<HttpResponseMessage> policy = 
                Policy.BulkheadAsync<HttpResponseMessage>(
                    maxParallelization:6,
                    maxQueuingActions:3,
                    onBulkheadRejectedAsync: context =>
                    {
                        _logger.LogWarning("Bulkhead Rejected. Too many concurrent requests.");
                        throw new BulkheadRejectedException("Too many concurrent requests. Please try again later.");

                    });
            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
        {
            AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .FallbackAsync(async (context)=>
                { 
                _logger.LogWarning("Fallback executed. Returning fallback response.");
                    HotelDTO hotel = new HotelDTO() {

                        HotelID = Guid.Empty,
                        HotelName= "Temporarily Unavailable",
                        HotelDescription = "Temporarily Unavailable",
                        HotelLocation = "Temporarily Unavailable",
                        Rooms = new List<RoomResponseDTO>()
                    };
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(hotel), Encoding.UTF8, "application/json")
                    };
                    return response;
                });
            return policy;
        }
    }
}
