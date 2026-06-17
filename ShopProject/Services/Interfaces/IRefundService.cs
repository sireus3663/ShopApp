using ShopProject.Models;

namespace ShopProject.Services.Interfaces
{
    public interface IRefundService
    {
        void CreateRequest(Guid orderId, int count, string reason);
        void Approve(Guid requestId, Guid moderatorId, string? comment = null);
        void Decline(Guid requestId, Guid moderatorId, string? comment = null);
        List<RefundRequest> GetUserRequests(Guid userId);
        List<RefundRequest> GetPendingRequests();
    }
}
