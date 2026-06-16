using System;

namespace ShopProject.Models
{
    public class Cart
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        public int Count { get; set; }

        public virtual Product? Product { get; set; }
    }
}