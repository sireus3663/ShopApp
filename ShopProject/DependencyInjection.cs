using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopProject.ConsoleCommands;
using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Forms;
using ShopProject.Forms.ViewModels;
using ShopProject.Services;
using ShopProject.Services.Interfaces;


namespace ShopProject
{
    public static class DependencyInjection
    {
        private static IServiceProvider _serviceProvider;
        private static readonly object _lock = new object();

        public static IServiceProvider ConfigureServices()
        {
            if (_serviceProvider != null)
                return _serviceProvider;

            lock (_lock)
            {
                if (_serviceProvider != null)
                    return _serviceProvider;

                var services = new ServiceCollection();

                services.AddSingleton<IAppConfigService, AppConfigService>();
                services.AddSingleton<ILoggerService, LoggerService>();

                services.AddDbContext<AppDbContext>((serviceProvider, options) =>
                {
                    var config = serviceProvider.GetRequiredService<IAppConfigService>();
                    var connectionString = config.GetConnectionString();
                    options.UseNpgsql(connectionString);
                });

                // Репозитории
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IProductRepository, ProductRepository>();
                services.AddScoped<ICartRepository, CartRepository>();
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IFavoriteRepository, FavoriteRepository>();
                services.AddScoped<IDiscountRepository, DiscountRepository>();
                services.AddScoped<IRefundRequestRepository, RefundRequestRepository>();

                // Сервисы
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IProductService, ProductService>();
                services.AddScoped<ICartService, CartService>();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<IFavoriteService, FavoriteService>();
                services.AddScoped<IDiscountService, DiscountService>();
                services.AddScoped<IModeratorService, ModeratorService>();
                services.AddScoped<IStatisticService, StatisticService>();
                services.AddScoped<IRefundService, RefundService>();

                // ViewModels
                services.AddScoped<MainViewModel>();
                services.AddScoped<AdminViewModel>();

                // Console
                services.AddScoped<ConsoleMode>();
                services.AddScoped<CommandRegistry>();

                // Forms
                services.AddTransient<MainForm>();
                services.AddTransient<LoginForm>();
                services.AddTransient<RegisterForm>();
                services.AddTransient<ProfileForm>();
                services.AddTransient<AdminPanelForm>();
                services.AddTransient<AdminConsoleForm>();
                services.AddTransient<ModerationForm>();
                services.AddTransient<CreateProductForm>();
                services.AddTransient<ProductDetailForm>();
                services.AddTransient<DbConnectionForm>();

                _serviceProvider = services.BuildServiceProvider();
                return _serviceProvider;
            }
        }

        public static T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public static T? GetServiceOrDefault<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}