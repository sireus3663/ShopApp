using ShopProject.ConsoleCommands.AuthServiceCommand;
using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.ConsoleCommands.CartServiceCommand;
using ShopProject.ConsoleCommands.FavoriteServiceCommand;
using ShopProject.ConsoleCommands.ModeratorServiceCommand;
using ShopProject.ConsoleCommands.OrderServiceCommand;
using ShopProject.ConsoleCommands.ProductServiceCommand;
using ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand;
using ShopProject.ConsoleCommands.UserServiceCommand;
using ShopProject.Db;
using ShopProject.Services;

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
            ModeratorService moderatorService,
            DiscountService discountService,
            ProductRepository productRepo,
            OrderRepository orderRepo,
            UserRepository userRepo,
            AppDbContext context
            )
        {
            // Базовые
            registry.Register(new EchoCommand());
            registry.Register(new HelpCommand(registry, authService)); 
            registry.Register(new ExitCommand());
            registry.Register(new ClearCommand());
            registry.Register(new MenuCommand(authService, registry));
            registry.Register(new ShowLogsCommand(logger));
            registry.Register(new TestErrorCommand(logger));

            // Auth
            registry.Register(new LoginCommand(authService));
            registry.Register(new LogoutCommand(authService));

            // User
            registry.Register(new RegisterCommand(userService));
            registry.Register(new ChangeRoleCommand(userService, authService, context));
            registry.Register(new ProfileCommand(userService, authService));
            registry.Register(new UsersCommand(userRepo, authService));  

            // Cart
            registry.Register(new AddToCartCommand(cartService, authService, productRepo));
            registry.Register(new RemoveFromCartCommand(authService, cartService));
            registry.Register(new ViewCartCommand(cartService, authService, productRepo));
            registry.Register(new BuyCartCommand(orderService, authService));

            // Favorite
            registry.Register(new ToggleFavoriteCommand(favoriteService, authService));
            registry.Register(new ViewFavoritesCommand(favoriteService, authService, productRepo));

            // Product
            registry.Register(new GetAllProductsCommand(productService));
            registry.Register(new SearchProductsCommand(productRepo));
            registry.Register(new GetByCategoryCommand(productRepo));
            registry.Register(new CreateProductCommand(productService, authService));
            registry.Register(new GetForModerateCommand(authService, productService));
            registry.Register(new ApproveProductCommand(productService, authService));
            registry.Register(new DeclineProductCommand(productService, authService));
            registry.Register(new EditProductCommand(productRepo, authService));  
            registry.Register(new DeleteProductCommand(productService, productRepo, authService));  
            registry.Register(new AddDiscountCommand(discountService, productRepo, authService));  
            registry.Register(new MyProductsCommand(productRepo, authService));
            

            // Order
            registry.Register(new GetUserOrdersCommand(orderService, authService));
            registry.Register(new ReturnOrderCommand(orderService, authService, orderRepo, userRepo));
            registry.Register(new ViewOrdersCommand(orderService, authService));

            // Moderator
            registry.Register(new ViewUserProfileCommand(moderatorService, authService));
            registry.Register(new ChangeBalanceCommand(moderatorService, authService));
            registry.Register(new ToggleBlockCommand(moderatorService, authService));

            // Statistic
            registry.Register(new TopProductsCommand(orderRepo, productRepo, authService));  
        }
    }
}