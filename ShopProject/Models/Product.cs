using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Guid? SellerId { get; set; }

    public string Category { get; set; } = string.Empty;

    public int Amount { get; set; }

    public bool IsApproved { get; set; }

    public byte[]? ProductImage { get; set; }
}