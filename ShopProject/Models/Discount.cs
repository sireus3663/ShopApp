using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models;

public class Discount
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public decimal Percent { get; set; }
}
