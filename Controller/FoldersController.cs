using AutoMapper;
using FileManagementPortal1.DTOs.Files;
using FileManagementPortal1.Models;
using FileManagementPortal1.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementPortal1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private readonly FolderRepository _folderRepository;
        private readonly IMapper _mapper;

        public FoldersController(FolderRepository folderRepository, IMapper mapper)
        {
            _folderRepository = folderRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FolderDto>>> GetFolders()
        {
            var folders = await _folderRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<FolderDto>>(folders));
        }

        [HttpGet("with-files")]
        public async Task<ActionResult<IEnumerable<FolderDto>>> GetFoldersWithFiles()
        {
            var folders = await _folderRepository.GetFoldersWithFilesAsync();
            return Ok(_mapper.Map<IEnumerable<FolderDto>>(folders));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FolderDto>> GetFolder(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);

            if (folder == null)
                return NotFound();

            return Ok(_mapper.Map<FolderDto>(folder));
        }

        [HttpGet("{id}/with-files")]
        public async Task<ActionResult<FolderDto>> GetFolderWithFiles(int id)
        {
            var folder = await _folderRepository.GetFolderWithFilesAsync(id);

            if (folder == null)
                return NotFound();

            return Ok(_mapper.Map<FolderDto>(folder));
        }

        [HttpPost]

        public async Task<ActionResult<FolderDto>> CreateFolder(FolderCreateDto folderCreateDto)
        {
            var folder = _mapper.Map<Folder>(folderCreateDto);
            var createdFolder = await _folderRepository.AddAsync(folder);

            return CreatedAtAction(nameof(GetFolder), new { id = createdFolder.Id }, _mapper.Map<FolderDto>(createdFolder));
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(int id, FolderCreateDto folderDto)
        {
            var folder = await _folderRepository.GetByIdAsync(id);

            if (folder == null)
                return NotFound(new { message = "Klasör bulunamadı." });

            _mapper.Map(folderDto, folder);

            // UpdateAsync Task türünde (void) dönüyor, sonuç alamayız
            await _folderRepository.UpdateAsync(folder);

            // Başarılı güncelleme yanıtı
            return Ok(new { success = true, message = "Klasör başarıyla güncellendi." });
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);

            if (folder == null)
                return NotFound(new { message = "Klasör bulunamadı." });

            // Silme işlemini gerçekleştirin
            await _folderRepository.DeleteAsync(folder);

            return Ok(new { message = "Klasör başarıyla silindi." });
        }

    }
}