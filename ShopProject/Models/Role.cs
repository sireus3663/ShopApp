using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models;

public class UserRole
{
    public Guid Id { get; set; }

    public string Role { get; set; }

    public Guid UserId { get; set; }
}
