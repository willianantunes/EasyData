using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Authentication.Entities
{
    [Table("auth_user")]
    public class AuthUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("password")]
        public string Password { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("is_superuser")]
        public bool IsSuperuser { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("date_joined")]
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }
}
