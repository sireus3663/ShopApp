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

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive =>
        (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) &&
        (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}
