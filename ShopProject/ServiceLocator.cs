using Microsoft.Extensions.DependencyInjection;
using ShopProject.Db;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject
{
    public static class ServiceLocator
    {
        private static ServiceProvider _serviceProvider;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            var services = new ServiceCollection();

            services.AddSingleton<AppDbContext>();

            services.AddScoped<UserRepository>();
            services.AddScoped<ProductRepository>();
            services.AddScoped<CartRepository>();
            services.AddScoped<OrderRepository>();
            services.AddScoped<FavoriteRepository>();
            services.AddScoped<DiscountRepository>();

            services.AddSingleton<LoggerService>();
            services.AddSingleton<AppConfigService>();
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<ProductService>();
            services.AddScoped<CartService>();
            services.AddScoped<OrderService>();
            services.AddScoped<FavoriteService>();
            services.AddScoped<DiscountService>();
            services.AddScoped<ModeratorService>();
            services.AddScoped<StartupService>();

            _serviceProvider = services.BuildServiceProvider();
            _isInitialized = true;
        }

        public static T GetService<T>() where T : notnull
        {
            if (!_isInitialized)
                throw new InvalidOperationException("ServiceLocator не инициализирован. Вызовите Initialize() первым.");

            return _serviceProvider.GetRequiredService<T>();
        }

        public static T GetServiceOrDefault<T>() where T : class
        {
            if (!_isInitialized)
                return null;

            return _serviceProvider.GetService<T>();
        }
    }
}
