using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms.ViewModels
{
    public class AdminViewModel
    {
        private readonly UserRepository _userRepo;
        private readonly OrderRepository _orderRepo;
        private readonly ProductRepository _productRepo;
        private readonly User _currentUser;

        public AdminViewModel(UserRepository userRepo, OrderRepository orderRepo, ProductRepository productRepo, User currentUser)
        {
            _userRepo = userRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _currentUser = currentUser;
        }

        public List<User> GetAllUsers()
        {
            return _userRepo.GetAll();
        }

        public List<User> SearchUsers(string searchText)
        {
            var users = GetAllUsers();
            if (string.IsNullOrWhiteSpace(searchText))
                return users;

            return users.Where(u => u.Name.Contains(searchText) || u.Email.Contains(searchText)).ToList();
        }

        public User GetUser(Guid userId)
        {
            return _userRepo.GetById(userId);
        }

        public void ChangeUserRole(Guid userId, Role newRole)
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Role = newRole;
            _userRepo.Update(user);
        }

        public void ToggleUserBlock(Guid userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            if (user.Id == _currentUser.Id)
                throw new Exception("Нельзя заблокировать себя");

            user.IsBlocked = !user.IsBlocked;
            _userRepo.Update(user);
        }

        public void ChangeUserBalance(Guid userId, decimal newBalance)
        {
            if (newBalance < 0)
                throw new Exception("Баланс не может быть отрицательным");

            var user = _userRepo.GetById(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Balance = newBalance;
            _userRepo.Update(user);
        }

        public List<Order> GetUserOrders(Guid userId)
        {
            return _orderRepo.GetByUser(userId);
        }

        public decimal GetUserTotalSpent(Guid userId)
        {
            return GetUserOrders(userId).Sum(o => o.Price);
        }

        public string GetProductName(Guid productId)
        {
            var product = _productRepo.GetById(productId);
            return product?.Name ?? "Неизвестно";
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string searchText)
        {
            var users = await GetAllUsersAsync();
            if (string.IsNullOrWhiteSpace(searchText))
                return users;

            return users.Where(u => u.Name.Contains(searchText) || u.Email.Contains(searchText)).ToList();
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            return await _userRepo.GetByIdAsync(userId);
        }

        public async Task ChangeUserRoleAsync(Guid userId, Role newRole)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Role = newRole;
            await _userRepo.UpdateAsync(user);
        }

        public async Task ToggleUserBlockAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            if (user.Id == _currentUser.Id)
                throw new Exception("Нельзя заблокировать себя");

            user.IsBlocked = !user.IsBlocked;
            await _userRepo.UpdateAsync(user);
        }

        public async Task ChangeUserBalanceAsync(Guid userId, decimal newBalance)
        {
            if (newBalance < 0)
                throw new Exception("Баланс не может быть отрицательным");

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Balance = newBalance;
            await _userRepo.UpdateAsync(user);
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _orderRepo.GetByUserAsync(userId);
        }

        public async Task<decimal> GetUserTotalSpentAsync(Guid userId)
        {
            var orders = await GetUserOrdersAsync(userId);
            return orders.Sum(o => o.Price);
        }

        public async Task<string> GetProductNameAsync(Guid productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            return product?.Name ?? "Неизвестно";
        }
    }
}