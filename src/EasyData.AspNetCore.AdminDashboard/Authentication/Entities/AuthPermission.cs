using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyData.AspNetCore.AdminDashboard.Authentication.Entities
{
    [Table("auth_permission")]
    public class AuthPermission
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("codename")]
        public string Codename { get; set; }
    }
}
