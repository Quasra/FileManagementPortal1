using System;
using System.ComponentModel.DataAnnotations;

namespace FileManagementPortal1.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; }

    }
}