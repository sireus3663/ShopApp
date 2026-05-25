using ShopProject.ConsoleCommands.AuthServiceCommand;
using ShopProject.ConsoleCommands.CartServiceCommand;
using ShopProject.ConsoleCommands.FavoriteServiceCommand;
using ShopProject.ConsoleCommands.OrderServiceCommand;
using ShopProject.ConsoleCommands.ProductServiceCommand;
using ShopProject.ConsoleCommands.UserServiceCommand;
using ShopProject.Db;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public static class CommandInitializer
    {
        public static void RegisterAll(
            CommandRegistry registry,
            LoggerService logger,
            AuthService authService,
            UserService userService,
            CartService cartService,
            FavoriteService favoriteService,
            ProductService productService,
            OrderService orderService,
            ProductRepository productRepo,
            AppDbContext context
            )
        {
            // Базовые
            registry.Register(new EchoCommand());
            registry.Register(new HelpCommand(registry));
            registry.Register(new ExitCommand());
            registry.Register(new ShowLogsCommand(logger));
            registry.Register(new TestErrorCommand(logger));

            // Auth
            registry.Register(new LoginCommand(authService));
            registry.Register(new LogoutCommand(authService));

            // User
            registry.Register(new RegisterCommand(userService));
            registry.Register(new ChangeRoleCommand(userService, authService, context));

            // Cart
            registry.Register(new AddToCartCommand(cartService, authService, productRepo));
            registry.Register(new RemoveFromCartCommand(authService, cartService));  
            registry.Register(new ViewCartCommand(cartService, authService, productRepo));

            // Favorite
            registry.Register(new ToggleFavoriteCommand(favoriteService, authService));

            // Product
            registry.Register(new GetAllProductsCommand(productService));
            registry.Register(new SearchProductsCommand(productRepo));
            registry.Register(new GetByCategoryCommand(productRepo));

            // Order
            registry.Register(new GetUserOrdersCommand(orderService, authService));
        }
        
    }
}
