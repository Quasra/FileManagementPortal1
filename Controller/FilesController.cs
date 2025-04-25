using AutoMapper;
using FileManagementPortal1.DTOs.Files;
using FileManagementPortal1.Models;
using FileManagementPortal1.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileManagementPortal1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly FolderRepository _folderRepository;
        private readonly IMapper _mapper;

        public FilesController(FileRepository fileRepository, FolderRepository folderRepository, IMapper mapper)
        {
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetUserFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var files = await _fileRepository.GetUserFilesAsync(userId);

            return Ok(_mapper.Map<IEnumerable<FileDto>>(files));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetAllFiles()
        {
            var files = await _fileRepository.GetAllIncludingAsync(f => f.User, f => f.Folder);

            return Ok(_mapper.Map<IEnumerable<FileDto>>(files));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileDto>> GetFile(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = await _fileRepository.GetByIdIncludingAsync(id, f => f.User, f => f.Folder);

            if (file == null)
                return NotFound();

            if (file.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(_mapper.Map<FileDto>(file));
        }

        [HttpPost("upload")]
        public async Task<ActionResult<FileDto>> UploadFile([FromForm] FileUploadDto fileUploadDto)
        {
            if (fileUploadDto.File == null || fileUploadDto.File.Length == 0)
                return BadRequest("No file uploaded");

            if (fileUploadDto.FolderId.HasValue)
            {
                var folderExists = await _folderRepository.ExistsAsync(fileUploadDto.FolderId.Value);
                if (!folderExists)
                    return BadRequest("Invalid folder id");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = await _fileRepository.UploadFileAsync(fileUploadDto.File, userId, fileUploadDto.FolderId);

            // Yeni eklenen dosyanın detaylı bilgilerini getir
            var fileWithDetails = await _fileRepository.GetByIdIncludingAsync(file.Id, f => f.User, f => f.Folder);

            return CreatedAtAction(nameof(GetFile), new { id = file.Id }, _mapper.Map<FileDto>(fileWithDetails));
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = await _fileRepository.GetByIdAsync(id);

            if (file == null)
                return NotFound();

            if (file.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var fileData = await _fileRepository.DownloadFileAsync(id);

            if (fileData.Item1 == null)
                return NotFound();

            return File(fileData.Item1, fileData.Item2, fileData.Item3);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = await _fileRepository.GetByIdAsync(id);

            if (file == null)
                return NotFound(new { message = "Dosya bulunamadı." });

            if (file.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _fileRepository.DeleteFileAsync(id, userId);

            if (!result)
                return BadRequest(new { message = "Dosya silinirken bir hata oluştu." });

            return Ok(new { message = $"'{file.FileName}' adlı dosya başarıyla silindi." });
        }


        private IActionResult HandleError(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}