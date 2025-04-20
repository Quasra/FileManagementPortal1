using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FileManagementPortal1.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for user's files
        public ICollection<FileModel> Files { get; set; }
    }
}