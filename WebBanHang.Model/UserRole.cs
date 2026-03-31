using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBanHang.Model;

namespace WebBanHang.Model
{
  
    [Table("user_roles")]
    public class UserRole
    {
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("role_id")]
        public short RoleId { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;
    }
}

