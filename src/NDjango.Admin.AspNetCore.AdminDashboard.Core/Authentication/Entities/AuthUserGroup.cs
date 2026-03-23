using System.ComponentModel.DataAnnotations.Schema;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Entities
{
    [Table("auth_user_groups")]
    public class AuthUserGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public AuthUser User { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }
        public AuthGroup Group { get; set; }
    }
}
