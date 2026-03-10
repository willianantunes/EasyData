using System.ComponentModel.DataAnnotations.Schema;

namespace EasyData.AspNetCore.AdminDashboard.Authentication.Entities
{
    [Table("auth_group_permissions")]
    public class AuthGroupPermission
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }
        public AuthGroup Group { get; set; }

        [Column("permission_id")]
        public int PermissionId { get; set; }
        public AuthPermission Permission { get; set; }
    }
}
