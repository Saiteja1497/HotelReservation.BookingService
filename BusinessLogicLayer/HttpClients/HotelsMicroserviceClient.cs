using DataAccessLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class HotelsMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HotelsMicroserviceClient> _logger;
        private readonly IDistributedCache _cache;
        public HotelsMicroserviceClient(HttpClient httpClient,ILogger<HotelsMicroserviceClient> logger,
            IDistributedCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<HotelDTO?> GetHotel(Guid hotelID)
        {
            try
            {
                if (hotelID == Guid.Empty)
                {
                    return null;
                }
                string cacheKey = $"Hotel_{hotelID}";
                string? cachedHotel = await _cache.GetStringAsync(cacheKey);
                if(!string.IsNullOrEmpty(cachedHotel))
                {
                    HotelDTO? hotelFromCache = JsonSerializer.Deserialize<HotelDTO>(cachedHotel);
                    if (hotelFromCache != null)
                    {
                        return hotelFromCache;
                    }
                }



                HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/hotels/search/hotel-id/{hotelID}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        HotelDTO? hotelFromFallBack = await response.Content.ReadFromJsonAsync<HotelDTO>();

                        if (hotelFromFallBack == null)
                        {
                            throw new NotImplementedException("Fallback was not implemented");
                        }
                        return hotelFromFallBack;
                    }
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);
                    }
                    else
                    {

                        return new HotelDTO()
                        {
                            HotelID = Guid.Empty,
                            HotelName = "Temporarily Unavailable",
                            HotelLocation = "Temporarily Unavailable",
                            HotelDescription = "Temporarily Unavailable",
                            Rooms = []
                        };
                    }
                }

                HotelDTO? hotel = await response.Content.ReadFromJsonAsync<HotelDTO>();

                if (hotel == null)
                {
                    throw new Exception("Invalid Hotel");
                }

                string hotelJson = JsonSerializer.Serialize(hotel);
                string cacheKeyToWrite = $"Hotel_{hotelID}";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(300));

                await _cache.SetStringAsync(cacheKeyToWrite, hotelJson, cacheOptions);


                return hotel;
            }
            catch(BulkheadRejectedException ex)
            {
                _logger.LogError(ex, "Bulkhead Rejected Exception in ProductsMicroserviceClient.GetProductByProductID");
               

                return new HotelDTO(){
                    HotelID = Guid.Empty,
                    HotelName = "Temporarily Unavailable",
                    HotelDescription = "Temporarily Unavailable",
                    HotelLocation = "Temporarily Unavailable",
                    Rooms = new List<RoomResponseDTO>()
                };
            }
        }
    }
}
