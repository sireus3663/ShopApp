using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;

namespace ShopProject.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRequestRepository _refundRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public RefundService(
            IRefundRequestRepository refundRepo,
            IOrderRepository orderRepo,
            IUserRepository userRepo,
            IAuthService authService,
            AppDbContext context)
        {
            _refundRepo = refundRepo;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _authService = authService;
            _context = context;
        }

        public void CreateRequest(Guid orderId, int count, string reason)
        {
            var user = _authService.RequireUser();

            var order = _orderRepo.GetById(orderId);
            if (order == null || order.UserId != user.Id)
                throw new Exception("Заказ не найден");

            if (count <= 0 || count > order.Count)
                throw new Exception("Некорректное количество для возврата");

            var refund = new RefundRequest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProductId = order.ProductId,
                OrderId = orderId,
                Count = count,
                Reason = reason,
                Status = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _refundRepo.Add(refund);
        }

        public void Approve(Guid requestId, Guid moderatorId, string? comment = null)
        {
            var request = _refundRepo.GetById(requestId);
            if (request == null)
                throw new Exception("Запрос на возврат не найден");
            if (request.Status != RefundStatus.Pending)
                throw new Exception("Запрос уже обработан");

            var order = _orderRepo.GetById(request.OrderId);
            if (order == null)
                throw new Exception("Заказ не найден");

            var user = _userRepo.GetById(request.UserId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            var refundAmount = order.Price / order.Count * request.Count;
            user.Balance += refundAmount;
            _userRepo.Update(user);

            request.Status = RefundStatus.Approved;
            request.ReviewedBy = moderatorId;
            request.ReviewComment = comment;
            _refundRepo.Update(request);
        }

        public void Decline(Guid requestId, Guid moderatorId, string? comment = null)
        {
            var request = _refundRepo.GetById(requestId);
            if (request == null)
                throw new Exception("Запрос на возврат не найден");
            if (request.Status != RefundStatus.Pending)
                throw new Exception("Запрос уже обработан");

            request.Status = RefundStatus.Declined;
            request.ReviewedBy = moderatorId;
            request.ReviewComment = comment;
            _refundRepo.Update(request);
        }

        public List<RefundRequest> GetUserRequests(Guid userId)
        {
            return _refundRepo.GetByUser(userId);
        }

        public List<RefundRequest> GetPendingRequests()
        {
            return _refundRepo.GetPending();
        }
    }
}
