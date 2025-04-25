using FileManagementPortal1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementPortal1.Repositories
{
    public class FolderRepository : GenericRepository<Folder>
    {
        public FolderRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<Folder>> GetFoldersWithFilesAsync()
        {
            return await GetQueryable()
                .Include(f => f.Files.Where(file => file.IsActive))
                .ToListAsync();
        }

        public async Task<Folder> GetFolderWithFilesAsync(int id)
        {
            return await GetQueryable()
                .Include(f => f.Files.Where(file => file.IsActive))
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task DeleteAsync(Folder folder)
        {
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }
    }
}