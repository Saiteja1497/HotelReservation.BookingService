using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services,IConfiguration configuration)
        {
            // Register business logic services here
            services.AddScoped<IHotelBookingService, HotelBookingService>();
            services.AddAutoMapper(cfg => { }, typeof(BookingAddRequestToBookingMappingProfile).Assembly);
            services.AddValidatorsFromAssemblyContaining<BookingAddRequestValidator>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";
            });
            services.AddTransient<IRabbitMQHotelNameUpdateConsumer, RabbitMQHotelNameUpdateConsumer>();
            services.AddHostedService<RabbitMQHotelNameUpdateHostedService>();

            services.AddTransient<IRabbitMQHotelDeleteConsumer, RabbitMQHotelDeleteConsumer>();
            services.AddHostedService<RabbitMQHotelDeleteHostedService>();

            return services;
        }
    }
}
