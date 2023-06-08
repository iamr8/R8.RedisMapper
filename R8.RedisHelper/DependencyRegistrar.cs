using Microsoft.Extensions.DependencyInjection;
using R8.RedisHelper.Handlers;

namespace R8.RedisHelper
{
    public static class DependencyRegistrar
    {
        /// <summary>
        /// Registers RedisHelper services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="options">An <see cref="RedisHelperOptions"/> to configure the provided <see cref="ICacheProvider"/>.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddRedisHelper(this IServiceCollection services, Action<RedisHelperOptions> options)
        {
            var redisHelperOptions = new RedisHelperOptions();
            options(redisHelperOptions);

            services.AddSingleton(redisHelperOptions);
            services.AddTransient<ICacheProvider, RedisCacheProvider>();

            return services;
        }
    }
}