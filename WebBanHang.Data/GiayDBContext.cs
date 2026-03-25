using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Model;

namespace WebBanHang.Data
{
    public class GiayDBContext : DbContext
    {
        public GiayDBContext(DbContextOptions<GiayDBContext> options) : base(options)
        {
        }

        public DbSet<Giay> Giays { get; set; }
    }
}
