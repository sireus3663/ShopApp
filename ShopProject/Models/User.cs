using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Name { get; set; }

    public string Password { get; set; }

    public decimal Balance { get; set; }

    public Role Role { get; set; }
}
