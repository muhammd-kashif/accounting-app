using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email required hai")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required hai")]
        [MinLength(6)]
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
