using Doppler.BillingUser.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
            return services;
        }
    }
}
