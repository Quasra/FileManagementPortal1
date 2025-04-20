using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FileManagementPortal1.DTOs.Files
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; }

        public int? FolderId { get; set; }
    }
}