using Microsoft.EntityFrameworkCore;
using ShopProject.Db.Interfaces;
using ShopProject.Models;

namespace ShopProject.Db
{
    public class RefundRequestRepository : BaseRepository<RefundRequest>, IRefundRequestRepository
    {
        public RefundRequestRepository(AppDbContext context) : base(context) { }

        public List<RefundRequest> GetByUser(Guid userId)
        {
            return _dbSet.Where(r => r.UserId == userId).ToList();
        }

        public List<RefundRequest> GetPending()
        {
            return _dbSet.Where(r => r.Status == RefundStatus.Pending).ToList();
        }

        public List<RefundRequest> GetAllWithDetails()
        {
            return _dbSet
                .Include(r => r.Product)
                .Include(r => r.Order)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }
}
