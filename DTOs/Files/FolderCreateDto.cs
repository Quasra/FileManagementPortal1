using System.ComponentModel.DataAnnotations;

namespace FileManagementPortal1.DTOs.Files
{
    public class FolderCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}