using DataAccessLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersMicroserviceClient> _logger; 
        private readonly IDistributedCache _cache;
        public UsersMicroserviceClient(HttpClient httpClient,ILogger<UsersMicroserviceClient> logger, IDistributedCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<UserDTO?> GetUser(Guid userID)
        {
            try
            {
                if (userID == Guid.Empty)
                {
                    return null;
                }
                string  cacheKey = $"UserID_{userID}";
                string? cachedUser = await _cache.GetStringAsync(cacheKey);
                if(!string.IsNullOrEmpty(cachedUser))
                {
                    UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);
                    if (userFromCache != null)
                    {
                        return userFromCache;
                    }
                }

                HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userID}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        UserDTO? userFromFallBack = await response.Content.ReadFromJsonAsync<UserDTO>();

                        if (userFromFallBack == null)
                        {
                            throw new NotImplementedException("Fallback was not implemented");
                        }
                        return userFromFallBack;
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
                        //throw new HttpRequestException($"Request failed with status code:{response.StatusCode}");
                        return new UserDTO(Email: "Temporarily Unavailable",
                              UserID: Guid.Empty,
                              UserName: "Temporarily Unavailable",
                              Gender: "Temporarily Unavailable"
                          );
                    }
                }

                UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

                if (user == null)
                {
                    throw new Exception("Invalid User ID");
                }

                string cacheKeyToWrite = $"UserID_{user.UserID}";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(3));
                string userToCache =  JsonSerializer.Serialize(user);
                await _cache.SetStringAsync(cacheKeyToWrite, userToCache, cacheOptions);

                return user;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError("Circuit is open. Fallback logic executed. Exception: {ExceptionMessage}", ex.Message);
                return new UserDTO(
                    Email: "Service Unavailable",
                              UserID: Guid.Empty,
                              UserName: "Service Unavailable",
                              Gender: "Service Unavailable");
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "Timeout occurred while fetching user data. Returning dummy data");

                return new UserDTO(
                        UserName: "Temporarily Unavailable (timeout)",
                        Email: "Temporarily Unavailable (timeout)",
                        Gender: "Temporarily Unavailable (timeout)",
                        UserID: Guid.Empty);
            }

        }
    }
}
