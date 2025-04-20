using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileManagementPortal1.Models
{
    public class FileModel : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public int? FolderId { get; set; }

        [ForeignKey("FolderId")]
        public Folder Folder { get; set; }
    }
}