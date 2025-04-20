using System;
using System.Collections.Generic;

namespace FileManagementPortal1.DTOs.Files
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public int FileCount { get; set; }
        public IEnumerable<FileDto> Files { get; set; }
    }
}