namespace ShopProject.Models;

public class RefundRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Count { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReviewedBy { get; set; }
    public string? ReviewComment { get; set; }

    public User? User { get; set; }
    public Product? Product { get; set; }
    public Order? Order { get; set; }
}

public enum RefundStatus
{
    Pending,
    Approved,
    Declined
}
