using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class MenuCommand : BaseCommand
    {
        private readonly AuthService _authService;

        public override string Name => "menu";
        public override string Description => "Показать главное меню";

        public MenuCommand(AuthService authService)
        {
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null)
            {
                ShowGuestMenu();
                return;
            }

            switch (_authService.currentUser.Role)
            {
                case Models.Role.Buyer:
                    ShowBuyerMenu();
                    break;
                case Models.Role.Seller:
                    ShowSellerMenu();
                    break;
                case Models.Role.Moderator:
                    ShowModeratorMenu();
                    break;
                case Models.Role.Admin:
                    ShowAdminMenu();
                    break;
                default:
                    ShowGuestMenu();
                    break;
            }
        }

        private void ShowGuestMenu()
        {
            Console.WriteLine("\n=== ГЛАВНОЕ МЕНЮ (ГОСТЬ) ===");
            Console.WriteLine("  login      - Вход в систему");
            Console.WriteLine("  register   - Регистрация нового пользователя");
            Console.WriteLine("  products   - Просмотр товаров");
            Console.WriteLine("  search     - Поиск товаров");
            Console.WriteLine("  category   - Товары по категории");
            Console.WriteLine("  help       - Показать все команды");
            Console.WriteLine("  exit       - Выход из программы");
        }

        private void ShowBuyerMenu()
        {
            Console.WriteLine($"\n=== ГЛАВНОЕ МЕНЮ (ПОКУПАТЕЛЬ) ===");
            Console.WriteLine($"Добро пожаловать, {_authService.currentUser.Name}!");
            Console.WriteLine("  profile          - Мой профиль");
            Console.WriteLine("  products         - Список товаров");
            Console.WriteLine("  search           - Поиск товаров");
            Console.WriteLine("  category         - Товары по категории");
            Console.WriteLine("  cart-add <id>    - Добавить товар в корзину");
            Console.WriteLine("  cart-remove <id> - Удалить товар из корзины");
            Console.WriteLine("  cart-view        - Показать корзину");
            Console.WriteLine("  favorite <id>    - Добавить/удалить из избранного");
            Console.WriteLine("  buy              - Оформить покупку");
            Console.WriteLine("  my-order         - Мои заказы");
            Console.WriteLine("  return <id>      - Вернуть заказ");
            Console.WriteLine("  logout           - Выход из системы");
            Console.WriteLine("  help             - Показать все команды");
        }

        private void ShowSellerMenu()
        {
            Console.WriteLine($"\n=== ГЛАВНОЕ МЕНЮ (ПРОДАВЕЦ) ===");
            Console.WriteLine($"Добро пожаловать, {_authService.currentUser.Name}!");
            Console.WriteLine("  profile        - Мой профиль");
            Console.WriteLine("  create-product - Создать товар");
            Console.WriteLine("  my-products    - Мои товары");              
            Console.WriteLine("  edit-product   - Редактировать товар");   
            Console.WriteLine("  add-discount   - Добавить скидку");      
            Console.WriteLine("  products       - Список товаров");
            Console.WriteLine("  search         - Поиск товаров");
            Console.WriteLine("  cart-view      - Просмотр корзины");
            Console.WriteLine("  buy            - Оформить покупку");
            Console.WriteLine("  my-order       - Мои заказы");
            Console.WriteLine("  logout         - Выход из системы");
            Console.WriteLine("  help           - Показать все команды");
        }

        private void ShowModeratorMenu()
        {
            Console.WriteLine($"\n=== ГЛАВНОЕ МЕНЮ (МОДЕРАТОР) ===");
            Console.WriteLine($"Добро пожаловать, {_authService.currentUser.Name}!");
            Console.WriteLine("  profile        - Мой профиль");
            Console.WriteLine("  moderate       - Товары на модерации");
            Console.WriteLine("  approve <id>   - Одобрить товар");
            Console.WriteLine("  decline <id>   - Отклонить товар");
            Console.WriteLine("  view-profile   - Просмотр профиля пользователя");
            Console.WriteLine("  set-balance    - Изменить баланс пользователя");
            Console.WriteLine("  block <email>  - Блокировка/разблокировка");
            Console.WriteLine("  products       - Список товаров");
            Console.WriteLine("  search         - Поиск товаров");
            Console.WriteLine("  logout         - Выход из системы");
            Console.WriteLine("  help           - Показать все команды");
        }

        private void ShowAdminMenu()
        {
            Console.WriteLine($"\n=== ГЛАВНОЕ МЕНЮ (АДМИНИСТРАТОР) ===");
            Console.WriteLine($"Добро пожаловать, {_authService.currentUser.Name}!");
            Console.WriteLine("  profile        - Мой профиль");
            Console.WriteLine("  users          - Список пользователей");   
            Console.WriteLine("  changerole     - Изменить роль пользователя");
            Console.WriteLine("  block <email>  - Блокировка/разблокировка");
            Console.WriteLine("  set-balance    - Изменить баланс пользователя");
            Console.WriteLine("  view-profile   - Просмотр профиля пользователя");
            Console.WriteLine("  moderate       - Товары на модерации");
            Console.WriteLine("  approve <id>   - Одобрить товар");
            Console.WriteLine("  decline <id>   - Отклонить товар");
            Console.WriteLine("  products       - Список товаров");
            Console.WriteLine("  search         - Поиск товаров");
            Console.WriteLine("  top-products   - Топ товаров по продажам");  
            Console.WriteLine("  add-discount   - Добавить скидку");          
            Console.WriteLine("  edit-product   - Редактировать товар");      
            Console.WriteLine("  delete-product - Удалить товар");          
            Console.WriteLine("  logout         - Выход из системы");
            Console.WriteLine("  help           - Показать все команды");
        }
    }
}