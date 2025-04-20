using Microsoft.AspNetCore.Identity;
using System;

namespace FileManagementPortal1.Models
{
    public class AppRole : IdentityRole
    {
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}