using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models;
public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public enum OrderStatus { Pending, Completed, Cancelled, Returned }
    public OrderStatus Status { get; set; } = OrderStatus.Completed;
    public DateTime? ReturnedAt { get; set; }
}