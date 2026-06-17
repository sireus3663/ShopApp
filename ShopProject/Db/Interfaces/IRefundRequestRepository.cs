using ShopProject.Models;

namespace ShopProject.Db.Interfaces
{
    public interface IRefundRequestRepository : IRepository<RefundRequest>
    {
        List<RefundRequest> GetByUser(Guid userId);
        List<RefundRequest> GetPending();
        List<RefundRequest> GetAllWithDetails();
    }
}
