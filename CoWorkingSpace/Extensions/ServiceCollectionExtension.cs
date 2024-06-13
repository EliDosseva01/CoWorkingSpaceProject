using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Services;
using CoWorkingSpace.Infrastructure.Data.Common;
using WorkingSpaceTest.Infrastructure.Data.Common;

namespace CoWorkingSpace.Extensions
{
    public static class ServiceCollectionExtension
    {
        //add services to builder
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<HttpClient, HttpClient>();   

            return services;
        }
    }
}
