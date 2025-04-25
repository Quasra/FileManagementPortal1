using AutoMapper;
using FileManagementPortal1.Models;
using FileManagementPortal1.DTOs.Account;
using FileManagementPortal1.DTOs.Files;

namespace FileManagementPortal1.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            //Hesaplar
            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<UserDto, AppUser>().ReverseMap();
            CreateMap<RegisterDto, AppUser>().ReverseMap();
            CreateMap<LoginDto, AppUser>().ReverseMap();
            CreateMap<UserUpdateDto, AppUser>().ReverseMap();
            CreateMap<AddToRoleDto, AppRole>().ReverseMap();

            // Dosyalar
            CreateMap<FileModel, FileDto>().ReverseMap();
            CreateMap<FileDto, FileModel>().ReverseMap();
            CreateMap<FileUploadDto, FileModel>().ReverseMap();

            // Klasörler
            CreateMap<Folder, FolderDto>().ReverseMap();
            CreateMap<FolderDto, Folder>().ReverseMap();
            CreateMap<FolderCreateDto, Folder>().ReverseMap();

        }
    }
}