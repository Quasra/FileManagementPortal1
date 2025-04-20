using System.ComponentModel.DataAnnotations;

namespace FileManagementPortal1.DTOs.Account
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}