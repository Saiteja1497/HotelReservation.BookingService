using BookingService.API.Middlewares;
using BusinessLogicLayer;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Policies;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddTransient<IIdentityMicroservicePolicies, IdentityMicroservicePolicies>();
builder.Services.AddTransient<IHotelMicroservicePolicies, HotelMicroservicePolicies>();
builder.Services.AddTransient<IPollyPolicies, PollyPolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
}).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IIdentityMicroservicePolicies>().GetCombinedPolicy()
    );

builder.Services.AddHttpClient<HotelsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["HotelsMicroserviceName"]}:{builder.Configuration["HotelsMicroservicePort"]}");
}).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IHotelMicroservicePolicies>().GetFallbackPolicy()
    ).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IHotelMicroservicePolicies>().GetBulkheadPolicy()
    );


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandlingMiddleware();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseRouting();
app.UseCors();

//app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
