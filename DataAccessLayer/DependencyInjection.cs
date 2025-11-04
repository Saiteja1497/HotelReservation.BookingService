using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DataAccessLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services,IConfiguration configuration)
        {
            // Register data access services here
            services.AddScoped<IBookingRepository, BookingRepository>();
            string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;
            string connectionString = connectionStringTemplate.Replace("$MONGO_HOST",Environment.GetEnvironmentVariable("MONGODB_HOST"))
                .Replace("$MONGO_PORT",Environment.GetEnvironmentVariable("MONGODB_PORT"));
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IMongoDatabase>(Provider =>
            {
                IMongoClient mongoClient = Provider.GetRequiredService<IMongoClient>();
                return mongoClient.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
            });

            services.AddScoped<IBookingRepository, BookingRepository>();
            return services;



        }
    }
}
