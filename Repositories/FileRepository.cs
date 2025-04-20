using FileManagementPortal1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementPortal1.Repositories
{
    public class FileRepository : GenericRepository<FileModel>
    {
        private readonly IWebHostEnvironment _environment;

        public FileRepository(ApplicationDbContext context, IWebHostEnvironment environment)
            : base(context)
        {
            _environment = environment;
        }

        public async Task<List<FileModel>> GetUserFilesAsync(string userId)
        {
            return await GetQueryable()
                .Include(f => f.User)
                .Include(f => f.Folder)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<FileModel> UploadFileAsync(IFormFile file, string userId, int? folderId = null)
        {
            // Create uploads directory if it doesn't exist
            string uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Create user directory if it doesn't exist
            string userFolder = Path.Combine(uploadsFolder, userId);
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            // Generate unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(userFolder, uniqueFileName);

            // Save file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Create file record in database
            var fileEntity = new FileModel
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FilePath = filePath,
                FileSize = file.Length,
                UserId = userId,
                FolderId = folderId
            };

            return await AddAsync(fileEntity);
        }

        public async Task<(byte[], string, string)> DownloadFileAsync(int id)
        {
            var file = await GetByIdAsync(id);
            if (file == null)
                return (null, null, null);

            byte[] fileBytes;

            try
            {
                fileBytes = await File.ReadAllBytesAsync(file.FilePath);
            }
            catch (Exception)
            {
                return (null, null, null);
            }

            return (fileBytes, file.ContentType, file.FileName);
        }

        public async Task<bool> DeleteFileAsync(int id, string userId)
        {
            var file = await GetByIdAsync(id);
            if (file == null || file.UserId != userId)
                return false;

            // Fiziksel dosyayı silme
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }

            // Dosya kaydını veritabanından silme (soft delete)
            return await SoftDeleteAsync(id);
        }
    }
}