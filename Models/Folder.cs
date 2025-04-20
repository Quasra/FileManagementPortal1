using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FileManagementPortal1.Models
{
    public class Folder : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation property
        public ICollection<FileModel> Files { get; set; }
    }
}