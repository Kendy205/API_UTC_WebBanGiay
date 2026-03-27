using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        public short RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        // ── Navigation Properties ──────────────────────────────
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

